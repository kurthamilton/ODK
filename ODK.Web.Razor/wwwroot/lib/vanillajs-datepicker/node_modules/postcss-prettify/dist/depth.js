'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = getDepth;
function getDepth(node) {
  var depth = 0;
  var parent = node.parent;
  while (parent.type !== 'root') {
    depth += 1;
    parent = parent.parent;
  }
  return depth;
}
module.exports = exports['default'];