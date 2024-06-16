'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = rule;

var _depth = require('../depth');

var _depth2 = _interopRequireDefault(_depth);

var _doubleSpace = require('../double-space');

var _doubleSpace2 = _interopRequireDefault(_doubleSpace);

var _indent = require('../indent');

var _indent2 = _interopRequireDefault(_indent);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function rule(node) {
  var nodeDepth = (0, _depth2.default)(node);
  (0, _indent2.default)(node, nodeDepth)(['before', 'after']);
  node.raws.between = ' ';
  node.raws.semicolon = true;
  if (node.selector.indexOf(', ') >= 0) {
    node.selector = node.selector.replace(/, /g, ',\n');
  }
  if (nodeDepth === 0) (0, _doubleSpace2.default)(node);
}
module.exports = exports['default'];