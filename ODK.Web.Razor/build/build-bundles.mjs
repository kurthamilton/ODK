/*
 * Builds the CSS and JavaScript bundles the layouts reference: concatenate a bundle's sources, then minify
 * the result as one script.
 *
 * Deliberately NOT esbuild's bundler. `--bundle` resolves a module graph and gives the result a scope, and
 * neither suits what is being bundled here:
 *
 *   - The vendored libraries are UMD builds. Inside an esbuild CommonJS wrapper they see `module`/`exports`
 *     and take the CommonJS branch, assigning to it instead of to `window` - so `window.bootstrap` and its
 *     friends would simply never appear.
 *   - odk.global.js declares a bare top-level `function setImageError`, which _MemberAvatar.cshtml calls from
 *     an inline `onerror=`. It is a global because it is at the top level of a plain script; put it in a
 *     scope and every avatar's error handler silently stops working.
 *
 * `transform` has neither problem: it minifies a script as a script, and does not rename anything declared at
 * the top level, because it cannot know what else refers to it.
 *
 * Run with `npm run build:bundles`. The csproj's BuildClientAssets target runs it on every build, after
 * the client libraries are restored - some bundles are built from wwwroot/lib, which is itself generated.
 *
 * There is deliberately no watch mode. A process rewriting wwwroot while MSBuild is evaluating the project
 * takes `dotnet watch` down with it, which is why Scripts/run.app.bat forbids a sass watcher alongside it -
 * a bundle watcher would be the same trap, and the csproj target already covers the case it would serve.
 *
 * The outputs are generated and gitignored, the same way the compiled CSS and wwwroot/lib are: a build
 * produces them, so nothing deployed depends on what was last built on a developer's machine. Nothing in
 * them should be edited by hand.
 */

import { transform, version as esbuildVersion } from 'esbuild';
import { readFile, stat, writeFile } from 'node:fs/promises';
import { dirname, join, posix, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

/*
 * One entry per bundle. `output` and `sources` are both relative to wwwroot, and a source path is the same
 * `lib/<library>/<file>` the views use - see build/copy-client-libs.mjs, which is what puts those there.
 *
 * Order matters and is the order given: these are plain scripts that run as they load, not modules with
 * declared dependencies, so a library has to precede whatever calls it.
 */
const BUNDLES = [
    {
        output: 'js/odk.bundle.js',
        sources: [
            'lib/cookieconsent/cookieconsent.min.js',
            'lib/bootstrap/js/bootstrap.bundle.js',
            'lib/flatpickr/flatpickr.js',
            'lib/aspnet-client-validation/dist/aspnet-validation.js',
            'lib/jscolor/jscolor.js',
            'lib/slim-select/slimselect.js',
            'js/odk.js',
            'js/odk.cookieconsent.js',
            'js/odk.currency-picker.js',
            'js/odk.dropdowns.js',
            'js/odk.feedback.js',
            'js/odk.forms.js',
            'js/odk.load.js',
            'js/odk.notifications.js',
            'js/odk.pagination.js',
            'js/odk.slim-select.js',
            'js/odk.selects.js',
            'js/odk.tabs.js',
            'js/odk.tasks.js',
            'js/odk.topics.js',
            'js/odk.html-editor.js'
        ]
    },
    {
        // Only the admin layouts load this - the scripts in it have no hooks on member-facing pages.
        output: 'js/odk.bundle.admin.js',
        sources: [
            'js/odk.email-preview.js',
            'js/odk.field-override.js',
            'js/odk.lists.js',
            'js/odk.bulk-email.js',
            'js/odk.placeholders.js'
        ]
    },
    {
        output: 'js/odk.bundle.head.js',
        sources: [
            'js/odk.global.js',
            'js/odk.themes.js'
        ]
    },
    {
        /* Its own bundle, pulled in by the three email template pages rather than added to the admin bundle:
           Ace is by far the largest script here and nothing else uses it, so every other admin page would pay
           for it. The mode and both themes travel alongside the core so Ace never fetches a module at runtime
           - that is what would otherwise need basePath set.

           Concatenated and not minified: these are Ace's own minified builds, so all the weight in here has
           been through a minifier already and running another over it saves nothing. */
        minify: false,
        output: 'js/odk.bundle.code-editor.js',
        sources: [
            'lib/ace/ace.js',
            'lib/ace/mode-html.js',
            'lib/ace/theme-textmate.js',
            'lib/ace/theme-monokai.js',
            'js/odk.code-editor.js'
        ]
    },
    {
        /* Its own bundle rather than an addition to the main one: only the payment confirm pages watch a
           checkout session, and the SignalR client is 47KB that every other page would carry for nothing. */
        output: 'js/odk.bundle.checkout.js',
        sources: [
            'lib/signalr/signalr.min.js',
            'js/odk.signalr.js',
            'js/odk.polling.js',
            'js/odk.checkout.js'
        ]
    },
    {
        output: 'css/odk.bundle.lib.css',
        sources: [
            'lib/font-awesome/css/all.css',
            'lib/flatpickr/flatpickr.css',
            'lib/aspnet-client-validation/dist/aspnet-validation.css',
            'lib/slim-select/slimselect.css'
        ]
    }
];

// Prepended after minifying, so it survives it. These files are checked into no history, so the banner is
// the only thing telling whoever opens one where it came from.
const BANNER = '/* Generated by build/build-bundles.mjs. Do not edit. */';

const projectDirectory = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const webRoot = join(projectDirectory, 'wwwroot');

await assertSourcesExist();
await buildAll();

console.log(`Bundled ${BUNDLES.length} bundles with esbuild ${esbuildVersion}.`);

async function assertSourcesExist() {
    const missing = [];

    for (const bundle of BUNDLES) {
        for (const source of bundle.sources) {
            try {
                await stat(join(webRoot, source));
            } catch {
                missing.push(source);
            }
        }
    }

    if (missing.length > 0) {
        throw new Error(
            'These bundle sources are missing from wwwroot. A lib/ path is produced by '
            + 'build/copy-client-libs.mjs, so check COPIES there first; anything else has been renamed or '
            + `deleted without build/build-bundles.mjs being updated:\n  ${missing.join('\n  ')}`);
    }
}

async function buildAll() {
    for (const bundle of BUNDLES) {
        await build(bundle);
    }
}

async function build(bundle) {
    const css = bundle.output.endsWith('.css');
    const parts = [];

    for (const source of bundle.sources) {
        /* LF-normalised as it is read: the bundle that is not minified concatenates CRLF files from the
           repo with LF files from npm, and it is committed, so a mixed result would differ from whatever
           Git checked out and show as modified for ever. esbuild emits LF for the minified ones regardless. */
        const contents = (await readFile(join(webRoot, source), 'utf8')).replace(/\r\n/g, '\n');
        parts.push(css ? absoluteUrls(contents, source) : contents);
    }

    /* Scripts are joined with a semicolon on its own line, not just a newline: a file whose last statement
       carries no terminator would otherwise be continued by the next file's opening parenthesis, which is how
       concatenating two perfectly good IIFEs produces one call to the first. Harmless where it is not needed -
       an empty statement is legal wherever a statement is. */
    let code = parts.join(css ? '\n' : '\n;\n');

    if (bundle.minify !== false) {
        const result = await transform(code, {
            loader: css ? 'css' : 'js',
            /* Third-party licence headers are `/*!` comments and stay: stripping the licence off somebody
               else's code to save a few bytes is not ours to do. */
            legalComments: 'inline',
            minify: true,
            target: 'es2020'
        });

        code = result.code;
    }

    await writeIfChanged(join(webRoot, bundle.output), `${BANNER}\n${code}`);
}

/*
 * Rewrites a stylesheet's relative url() references to absolute ones, because the bundle is served from a
 * different directory than the file the reference was written in: Font Awesome asks for `../webfonts/x.woff2`
 * from `/lib/font-awesome/css/`, which resolves to nothing at all from `/css/`. Absolute paths are
 * independent of where the bundle ends up, which is the property worth having.
 */
function absoluteUrls(css, source) {
    const sourceDirectory = posix.dirname(source);

    return css.replace(
        /url\(\s*(['"]?)([^'")]+)\1\s*\)/g,
        (match, quote, url) => {
            // A data: or absolute URL already resolves from anywhere, and a fragment names something in-document.
            if (/^([a-z][a-z0-9+.-]*:|\/|#)/i.test(url)) {
                return match;
            }

            return `url(${quote}/${posix.normalize(posix.join(sourceDirectory, url))}${quote})`;
        });
}

/*
 * Writes only a changed file. The outputs are committed and are static web assets, so rewriting one that has
 * not changed dirties the working tree's timestamps and makes the build re-hash it for nothing.
 */
async function writeIfChanged(path, contents) {
    try {
        if (await readFile(path, 'utf8') === contents) {
            return;
        }
    } catch {
        // No such file yet, which is a change.
    }

    await writeFile(path, contents, 'utf8');
    console.log(`  ${relative(projectDirectory, path)} (${contents.length.toLocaleString()} bytes)`);
}
