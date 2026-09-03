/*
 * Copies the client-side libraries the app serves out of node_modules into wwwroot/lib.
 *
 * Every package publishes its own layout, while the views and the bundles in build/build-bundles.mjs
 * reference `lib/<library>/<file>`. COPIES maps the former onto the latter, so a served path is a property
 * of this file rather than of whatever shape a package happens to publish - which is what lets a package be
 * upgraded, or swapped for another source of the same library, without touching a Razor file.
 *
 * COPIES is also the "these files and no more" contract: only what is listed here reaches wwwroot/lib, so a
 * package's tests, sources, TypeScript declarations and unused builds stay out of the deploy.
 *
 * Run with `npm run build:lib`. The csproj runs it on every build, so wwwroot/lib is a build output: it is
 * gitignored, rebuilt from scratch whenever the inputs change, and nothing in it should be edited by hand.
 * Pass --force to rebuild it when nothing has changed.
 */

import { createHash } from 'node:crypto';
import { cp, mkdir, readdir, readFile, rm, stat, writeFile } from 'node:fs/promises';
import { dirname, join, posix, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

/*
 * [source, destination, filter?] - source relative to node_modules, destination relative to wwwroot/lib.
 * Either may be a file or a directory; a directory is copied whole, so a package that resolves assets at
 * runtime (see the notes below) keeps the tree it expects to find. The optional filter narrows a directory
 * copy - see minifiedOnly, the only one so far.
 *
 * Versions are not stated here - they live in package.json, pinned exactly, and package-lock.json resolves
 * them. Adding a library means adding the dependency and a line or two here.
 */
const COPIES = [
    /* Only the bundle build is served - it is the one build/build-bundles.mjs names. scss is
       here because wwwroot/scss imports Bootstrap's own sources directly (see scss/bootstrap/main.scss)
       rather than overriding compiled CSS, so the package's compiled dist/css is never asked for. */
    ['bootstrap/dist/js/bootstrap.bundle.js', 'bootstrap/js/bootstrap.bundle.js'],
    ['bootstrap/scss', 'bootstrap/scss'],

    /* all.css is self-contained - it imports nothing and references only the four woff2 files in webfonts,
       so the two travel together. The package's js, sprites and svgs directories deliver the same icons as
       script or as inline SVG, which is a different way of using Font Awesome than the webfont this app
       loads, and is most of the package's weight. */
    ['@fortawesome/fontawesome-free/css/all.css', 'font-awesome/css/all.css'],
    ['@fortawesome/fontawesome-free/webfonts', 'font-awesome/webfonts'],

    // The dist segment is kept: the bundles reference lib/aspnet-client-validation/dist/...
    ['aspnet-client-validation/dist/aspnet-validation.css', 'aspnet-client-validation/dist/aspnet-validation.css'],
    ['aspnet-client-validation/dist/aspnet-validation.js', 'aspnet-client-validation/dist/aspnet-validation.js'],

    // No locale and no plugin is configured (see odk.forms.js), so the core script and its stylesheet are
    // the whole of what flatpickr needs.
    ['flatpickr/dist/flatpickr.css', 'flatpickr/flatpickr.css'],
    ['flatpickr/dist/flatpickr.js', 'flatpickr/flatpickr.js'],

    ['slim-select/dist/slimselect.css', 'slim-select/slimselect.css'],
    ['slim-select/dist/slimselect.js', 'slim-select/slimselect.js'],

    // The ES module build, which _Layout imports directly rather than through a bundle.
    ['cropperjs/dist/cropper.esm.min.js', 'cropperjs/cropper.esm.min.js'],

    // Script only - the banner is styled by wwwroot/scss/_cookieconsent.scss, not by the package's CSS.
    ['cookieconsent/build/cookieconsent.min.js', 'cookieconsent/cookieconsent.min.js'],

    ['@eastdesire/jscolor/jscolor.js', 'jscolor/jscolor.js'],

    /* TinyMCE resolves skins, themes, plugins, models and icons off tinymce.baseURL at runtime, and
       odk.html-editor.js builds content_css from it for the dark theme, so every skin and plugin has to be
       here - which one is asked for is not referenced from anywhere a build could see.

       Which *build* of each is, though: tinymce.min.js has the '.min' suffix baked in, so it only ever
       requests plugin.min.js, skin.min.css, theme.min.js and their like. minifiedOnly drops the rest, which
       is the unminified duplicate of every file, the index.js ESM entries a bundler would use, and the
       TypeScript sources beside the skin stylesheets - two thirds of the tree, and the largest single
       saving in the deployed payload.

       Two plugins ship a non-min data payload their minified build fetches - emoticons/js/emojis.js and
       help/js/i18n/keynav - so adding either to the plugin list in odk.html-editor.js means excepting it
       here. Neither is configured. */
    ['tinymce/icons', 'tinymce/icons', minifiedOnly],
    ['tinymce/models', 'tinymce/models', minifiedOnly],
    ['tinymce/plugins', 'tinymce/plugins', minifiedOnly],
    ['tinymce/skins', 'tinymce/skins', minifiedOnly],
    ['tinymce/themes', 'tinymce/themes', minifiedOnly],
    ['tinymce/tinymce.min.js', 'tinymce/tinymce.min.js'],

    /* Ace would lazily fetch a mode or a theme from its base path, but the code-editor bundle concatenates
       the one mode and the two themes the app uses alongside the core, so it never asks. src-min-noconflict
       is Ace's minified build that leaves globals other than `ace` alone. */
    ['ace-builds/src-min-noconflict/ace.js', 'ace/ace.js'],
    ['ace-builds/src-min-noconflict/mode-html.js', 'ace/mode-html.js'],
    ['ace-builds/src-min-noconflict/theme-monokai.js', 'ace/theme-monokai.js'],
    ['ace-builds/src-min-noconflict/theme-textmate.js', 'ace/theme-textmate.js'],

    /* The browser build, which is a self-contained UMD bundle and the only one served - the ES modules,
       the Node build and the TypeScript declarations beside it are for a bundler this app does not use. */
    ['@microsoft/signalr/dist/browser/signalr.min.js', 'signalr/signalr.min.js'],

    ['mermaid/dist/mermaid.min.js', 'mermaid/mermaid.min.js']
];

// Records what wwwroot/lib was built from, so a build with nothing to do costs one file read.
const FINGERPRINT_FILE = '.client-libs.json';

const scriptDirectory = dirname(fileURLToPath(import.meta.url));
const projectDirectory = resolve(scriptDirectory, '..');
const nodeModules = join(projectDirectory, 'node_modules');
const libDirectory = join(projectDirectory, 'wwwroot', 'lib');
const fingerprintPath = join(libDirectory, FINGERPRINT_FILE);

await main();

async function main() {
    const force = process.argv.includes('--force');
    const fingerprint = await buildFingerprint();

    if (!force && await isUpToDate(fingerprint)) {
        console.log('wwwroot/lib is up to date.');
        return;
    }

    await assertSourcesExist();

    // Rebuilt rather than merged into, so a library dropped from COPIES leaves nothing behind.
    await rm(libDirectory, { recursive: true, force: true });

    for (const [source, destination, filter] of COPIES) {
        const target = join(libDirectory, destination);
        await mkdir(dirname(target), { recursive: true });
        await cp(join(nodeModules, source), target, { recursive: true, filter });
    }

    await writeFile(fingerprintPath, JSON.stringify(fingerprint, null, 2) + '\n');

    const { files, bytes } = await measure(libDirectory);
    console.log(`Copied ${files} files (${(bytes / 1024 / 1024).toFixed(1)} MB) to wwwroot/lib.`);
}

/* A COPIES filter keeping only minified builds. Directories are always kept, or the walk stops at the first
   one and takes the tree with it; the decision is per file. Returning a promise is supported, so the stat
   that tells the two apart can be awaited. */
async function minifiedOnly(source) {
    return (await stat(source)).isDirectory() || /\.min\.[^.]+$/.test(source);
}

/* The installed version of every package copied from, plus a hash of this file - between them they cover
   every input, the copy being a pure function of the two. */
async function buildFingerprint() {
    const packages = {};

    for (const name of packageNames()) {
        const manifest = JSON.parse(await readFile(join(nodeModules, name, 'package.json'), 'utf8'));
        packages[name] = manifest.version;
    }

    const source = await readFile(fileURLToPath(import.meta.url));

    return { script: createHash('sha256').update(source).digest('hex'), packages };
}

async function isUpToDate(fingerprint) {
    try {
        const current = JSON.parse(await readFile(fingerprintPath, 'utf8'));
        return current.script === fingerprint.script
            && JSON.stringify(current.packages) === JSON.stringify(fingerprint.packages);
    } catch {
        // Absent, or written by a version that shaped it differently - either way, build.
        return false;
    }
}

/* Checked up front, and all of them, so an upgrade that moves a file reports every path it broke in one go.
   A missing source is otherwise silent until the browser asks for it. */
async function assertSourcesExist() {
    const missing = [];

    for (const [source] of COPIES) {
        try {
            await stat(join(nodeModules, source));
        } catch {
            missing.push(source);
        }
    }

    if (missing.length > 0) {
        throw new Error(
            'These paths are missing from node_modules - check whether an upgrade moved them, and update '
            + `build/copy-client-libs.mjs:\n  ${missing.join('\n  ')}`);
    }
}

async function measure(directory) {
    const entries = await readdir(directory, { recursive: true, withFileTypes: true });
    let files = 0;
    let bytes = 0;

    for (const entry of entries) {
        if (!entry.isFile()) {
            continue;
        }

        files++;
        bytes += (await stat(join(entry.parentPath, entry.name))).size;
    }

    return { files, bytes };
}

// The package a source path belongs to: its first segment, or the first two when the package is scoped.
function packageNames() {
    const names = COPIES.map(([source]) => {
        const segments = posix.normalize(source).split('/');
        return segments[0].startsWith('@') ? `${segments[0]}/${segments[1]}` : segments[0];
    });

    return [...new Set(names)];
}
