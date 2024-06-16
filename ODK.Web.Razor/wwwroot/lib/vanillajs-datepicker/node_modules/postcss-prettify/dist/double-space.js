'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = doubleSpace;
function doubleSpace(node) {
  node.raws.before += '\n';
}
module.exports = exports['default'];