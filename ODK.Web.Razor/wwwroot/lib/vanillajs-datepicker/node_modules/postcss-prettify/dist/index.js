'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});

var _postcss = require('postcss');

var _postcss2 = _interopRequireDefault(_postcss);

var _format = require('./format');

var _format2 = _interopRequireDefault(_format);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

var prettify = _postcss2.default.plugin('postcss-prettify', function () {
  return function (css) {
    css.walk(_format2.default);
    if (css.first && css.first.raws) css.first.raws.before = '';
  };
});

prettify.process = function (css) {
  return (0, _postcss2.default)([prettify]).process(css);
};

exports.default = prettify;
module.exports = exports['default'];