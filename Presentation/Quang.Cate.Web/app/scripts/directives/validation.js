(function (window, angular, undefined) {
    'use strict';

    var app = angular.module('quangcatewebApp');

    /**
     * @ngdoc directive
     * @name danhmucApp.directive:validSubmit
     * @description
     * # validSubmit
     */
    app.directive('validSubmit', ['$parse', function ($parse) {
        var linkFn = function ($scope, el, attrs, formCtrl) {
            var fn = $parse(attrs.validSubmit);// alert(fn);
            var form = el.controller('form');
            form.$submitted = false;
            el.on('submit', function (event) {
                $scope.$apply(function () {
                    el.addClass('ng-submitted');
                    form.$submitted = true;
                    if (form.$valid) {
                        fn($scope, { $event: event });
                    } else {

                        //element.removeClass('has-error').addClass('has-success');
                        var formGroups = el[0].querySelectorAll(".form-group");
                        //alert(inputEl);
                        var first = true;
                        angular.forEach(formGroups, function (formGroup) {
                            var inputEl = formGroup.querySelector("[name]");
                            var inputNgEl = angular.element(inputEl);
                            var inputName = inputNgEl.attr('name');
                            if (inputName && formCtrl[inputName].$invalid) {
                                angular.element(formGroup).addClass('has-error');
                                if (first && inputNgEl.focus) {
                                    inputNgEl.focus();
                                    first = false;
                                }
                            }
                        });
                    }
                });
            });
        };
        return {
            restrict: 'A',
            require: '^form',
            compile: function (elem, attrs) {
                return linkFn;
            }
        }
    }]);

    /**
     * @ngdoc directive
     * @name danhmucApp.directive:showErrors
     * @description
     * # showErrors
     */
    app.directive('showErrors', ['$parse', function ($parse) {

        var linkFn = function ($scope, el, attrs, formCtrl) {

            // find the text box element, which has the 'name' attribute
            var inputEl = el[0].querySelector("[name]");

            // convert the native text box element to an angular element
            var inputNgEl = angular.element(inputEl);

            // only apply the has-error class after the user leaves the text box
            inputNgEl.bind('blur', function () {
                setTimeout(function () {
                    // get the name on the text box so we know the property to check
                    // on the form controller
                    var inputName = inputNgEl.attr('name');
                    if (inputName && formCtrl[inputName]) {
                        el.toggleClass('has-error', formCtrl[inputName].$invalid);
                    }
                }, 300);
            });
        };
        return {
            restrict: 'A',
            require: '^form',
            compile: function (elem, attrs) {
                if (!elem.hasClass('form-group')) {
                    throw 'show-errors element does not have the \'form-group\' class';
                }
                return linkFn;
            }
        }
    }]);

    //app.directive('checkExistGa', ['$parse', 'gaService', function ($parse, gaService) {
    //    return {
    //        require: ['^form', 'ngModel'],
    //        restrict: 'A',
    //        link: function ($scope, element, attrs, ctrls) {
    //            var gaId;
    //            var formCtrl = ctrls[0];
    //            var modelCtrl = ctrls[1];
    //            attrs.$observe('checkExistGa', function (value) {
    //                gaId = value;
    //                // Check on first load
    //                applyValidateFn(angular.element(element[0]).val());
    //            });

    //            var applyValidateFn = function (value) {
    //                modelCtrl.$setValidity('checkExistGa', true);
    //                if (value !== "" && typeof value !== "undefined") {
    //                    // Only check if valid ma ga
    //                    if (value.length >= 2) {
    //                        gaService.existGa(value, gaId, function (isExist) {
    //                            modelCtrl.$setValidity('checkExistGa', !isExist);
    //                        });

    //                    }
    //                }
    //                return value;
    //            };
    //            modelCtrl.$parsers.push(applyValidateFn);
    //        }
    //    }
    //}]);
    // check exit dau may
    //app.directive('checkExistDauMay', ['$parse', 'daumayService', function ($parse, gaService) {
    //    return {
    //        require: ['^form', 'ngModel'],
    //        restrict: 'A',
    //        link: function ($scope, element, attrs, ctrls) {
    //            var daumayId;
    //            var formCtrl = ctrls[0];
    //            var modelCtrl = ctrls[1];
    //            attrs.$observe('checkExistDauMay', function (value) {
    //                daumayId = value;
    //                // Check on first load
    //                applyValidateFn(angular.element(element[0]).val());
    //            });

    //            var applyValidateFn = function (value) {
    //                modelCtrl.$setValidity('checkExistDauMay', true);
    //                if (value !== "" && typeof value !== "undefined") {
    //                    // Only check if valid ma ga
    //                    if (value.length >= 2) {
    //                        daumayService.existDauMay(value, daumayId, function (isExist) {
    //                            modelCtrl.$setValidity('checkExistDauMay', !isExist);
    //                        });

    //                    }
    //                }
    //                return value;
    //            };
    //            modelCtrl.$parsers.push(applyValidateFn);
    //        }
    //    }
    //}]);
})(window, window.angular);