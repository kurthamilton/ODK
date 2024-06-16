'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = atrule;

var _depth = require('../depth');

var _depth2 = _interopRequireDefault(_depth);

var _doubleSpace = require('../double-space');

var _doubleSpace2 = _interopRequireDefault(_doubleSpace);

var _indent = require('../indent');

var _indent2 = _interopRequireDefault(_indent);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

/**
 * Append space to colon if necessary. See at-rule-spacing-colon test case.
 */
var params = {
  match: /(\(.*)(:)([^\s])(.*\))/g,
  replace: '$1$2 $3$4'
};

function atrule(node) {
  var nodeDepth = (0, _depth2.default)(node);
  (0, _indent2.default)(node, nodeDepth)(['before', 'after']);
  node.raws.between = node.nodes ? ' ' : '';
  if (node.params) {
    node.raws.afterName = ' ';
    node.params = node.params.replace(params.match, params.replace);
  }
  if (nodeDepth === 0) (0, _doubleSpace2.default)(node);
}
module.exports = exports['default'];