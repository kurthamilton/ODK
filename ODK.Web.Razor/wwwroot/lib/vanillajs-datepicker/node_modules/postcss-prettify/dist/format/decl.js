'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = decl;

var _depth = require('../depth');

var _depth2 = _interopRequireDefault(_depth);

var _indent = require('../indent');

var _indent2 = _interopRequireDefault(_indent);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function decl(node) {
  (0, _indent2.default)(node, (0, _depth2.default)(node))(['before']);
  node.raws.between = ': ';
}
module.exports = exports['default'];