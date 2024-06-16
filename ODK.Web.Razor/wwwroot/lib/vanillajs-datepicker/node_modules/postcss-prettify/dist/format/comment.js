'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = comment;

var _depth = require('../depth');

var _depth2 = _interopRequireDefault(_depth);

var _doubleSpace = require('../double-space');

var _doubleSpace2 = _interopRequireDefault(_doubleSpace);

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function comment(node) {
  if ((0, _depth2.default)(node) === 0) (0, _doubleSpace2.default)(node);
}
module.exports = exports['default'];