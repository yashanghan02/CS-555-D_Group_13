 /***/ "./src/characterControls.ts":
      /*!**********************************!*\
  !*** ./src/characterControls.ts ***!
  \**********************************/
      /***/ (__unused_webpack_module, exports, __webpack_require__) => {
        Object.defineProperty(exports, "__esModule", { value: true });
        exports.CharacterControls = void 0;

  Object.defineProperty(exports, "__esModule", { value: true });

  
      exports.KeyDisplay =
          exports.DIRECTIONS =
          exports.SHIFT =
          exports.D =
          exports.S =
          exports.A =
          exports.W =
            void 0;
        exports.W = "w";
        exports.A = "a";
        exports.S = "s";
        exports.D = "d";
        exports.SHIFT = "shift";
        exports.DIRECTIONS = [exports.W, exports.A, exports.S, exports.D];
        var KeyDisplay = /** @class */ (function () {
          function KeyDisplay() {
            this.map = new Map();
            var w = document.createElement("div");
            var a = document.createElement("div");
            var s = document.createElement("div");
            var d = document.createElement("div");
            var shift = document.createElement("div");
            this.map.set(exports.W, w);
            this.map.set(exports.A, a);
            this.map.set(exports.S, s);
            this.map.set(exports.D, d);
            this.map.set(exports.SHIFT, shift);
            this.map.forEach(function (v, k) {
              v.style.color = "blue";
              v.style.fontSize = "50px";
              v.style.fontWeight = "800";
              v.style.position = "absolute";
              v.textContent = k;
            });
            this.updatePosition();
            this.map.forEach(function (v, _) {
              document.body.append(v);
            });
          }