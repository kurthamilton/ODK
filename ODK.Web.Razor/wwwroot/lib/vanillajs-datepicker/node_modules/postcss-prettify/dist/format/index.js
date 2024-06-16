'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = format;

var _atrule = require('./atrule');

var _atrule2 = _interopRequireDefault(_atrule);

var _comment = require('./comment');

var _comment2 = _interopRequireDefault(_comment);

var _decl = require('./decl');

var _decl2 = _interopRequireDefault(_decl);

var _rule = require('./rule');

var _rule2 = _interopRequireDefault(_rule);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function format(node) {
  return { atrule: _atrule2.default, comment: _comment2.default, decl: _decl2.default, rule: _rule2.default }[node.type](node);
}
module.exports = exports['default'];