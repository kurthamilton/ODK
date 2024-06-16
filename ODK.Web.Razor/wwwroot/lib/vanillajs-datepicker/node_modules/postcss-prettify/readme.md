# `postcss-prettify` [![version][1]][2] [![build][3]][4] [![coverage][5]][6]

<b>[About](#about)</b> |
<b>[Installation](#installation)</b> |
<b>[Usage](#usage)</b> |
<b>[License](#license)</b>

---

## About

A [PostCSS](https://github.com/postcss/postcss) plugin to prettify output. Requires at least Node.js v0.12. Should likely be included towards the end of a PostCSS plugin chain, if that's your jam.

**Features**
- line breaks between top-level rules and comments
- smart spacing around rules and declarations
- indenting with 2 spaces
- one selector per line

**Example Input**
```css
.foo, .bar {
    background: red;
}
@media only screen and (min-width:600px){.baz{background:blue;}}
```

**Example Output**
```css
.foo,
.bar {
  background: red;
}

@media only screen and (min-width: 600px) {
  .baz {
    background: blue;
  }
}
```

## Installation

**From a terminal**

```sh
npm install --save-dev postcss-prettify
```

## Usage

**As a PostCSS Plugin**
```js
postcss([
  require('postcss-prettify')
])
```

Check the [PostCSS docs](https://github.com/postcss/postcss#usage) for your chosen implementation.

**Standalone**

`postcss-prettify` also exposes a standalone [PostCSS processor instance](https://github.com/postcss/postcss/blob/master/docs/api.md#processorprocesscss-opts) as `prettify.process(css)`:

```js
var fs = require('mz/fs')
var prettify = require('postcss-prettify')

fs.readFile('src/style.css', 'utf8')
  .then(data => prettify.process(data))
  .then(res => fs.writeFile('dist/style.css', res.css))
  .catch(err => console.err(err.stack))
```

## License

[MIT](https://github.com/codekirei/postcss-prettify/blob/master/license)

[1]: https://img.shields.io/npm/v/postcss-prettify.svg?style=flat-square&label=version
[2]: https://www.npmjs.com/package/postcss-prettify
[3]: https://img.shields.io/travis/codekirei/postcss-prettify.svg?style=flat-square&label=tests
[4]: https://travis-ci.org/codekirei/postcss-prettify
[5]: http://img.shields.io/coveralls/codekirei/postcss-prettify.svg?style=flat-square
[6]: https://coveralls.io/github/codekirei/postcss-prettify?branch=master
