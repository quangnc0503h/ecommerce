'use strict';

/**
 * @ngdoc directive
 * @name danhmucApp.directive:validSubmit
 * @description
 * # validSubmit
 */
angular.module('quangauthwebApp')
.directive('validSubmit', ['$parse', function ($parse) {

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
        /*
        $scope.$watch(function () { return form.$valid }, function (isValid) {
            if (form.$submitted == false) return;
            if (isValid) {
                el.removeClass('has-error').addClass('has-success');
            } else {
                el.removeClass('has-success');
                el.addClass('has-error');
            }
        });
        */
    };
    return {
        restrict: 'A',
        require: '^form',
        compile: function (elem, attrs) {
            return linkFn;
        }
    }
}])

/**
 * @ngdoc directive
 * @name danhmucApp.directive:showErrors
 * @description
 * # showErrors
 */
.directive('showErrors', ['$parse', function ($parse) {

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
        /*
        var form = el.controller('form');
        var fn = $parse(attrs.showErrors);// alert(fn);
        el.on('submit', function (event) {
            $scope.$apply(function () {
                el.addClass('ng-submitted');
                alert(form);
                form.$submitted = true;
                if (form.$valid) {
                    fn($scope, { $event: event });
                }
            });
        });
        */
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
}])
.directive("passwordVerify", function () {
    return {
        require: "ngModel",
        scope: {
            passwordVerify: '='
        },
        link: function (scope, element, attrs, ctrl) {
            scope.$watch(function () {
                var combined;

                if (scope.passwordVerify || ctrl.$viewValue) {
                    combined = scope.passwordVerify + '_' + ctrl.$viewValue;
                }
                return combined;
            }, function (value) {
                if (value) {
                    ctrl.$parsers.unshift(function (viewValue) {
                        var origin = scope.passwordVerify;
                        if (origin !== viewValue) {
                            ctrl.$setValidity("passwordVerify", false);
                            return undefined;
                        } else {
                            ctrl.$setValidity("passwordVerify", true);
                            return viewValue;
                        }
                    });
                }
            });
        }
    };
})
.directive('checkExistUsername', ['$parse', 'userService', function ($parse, userService) {

    return {
        require: ['^form', 'ngModel'],
        restrict: 'A',
        link: function ($scope, element, attrs, ctrls) {
            var userId;
            var formCtrl = ctrls[0];
            var modelCtrl = ctrls[1];
            attrs.$observe('checkExistUsername', function (value) {
                userId = value;
                // Check on first load
                applyValidateFn(angular.element(element[0]).val());
            });

            var applyValidateFn = function (value) {
                modelCtrl.$setValidity('checkExistUsername', true);
                if (value !== "" && typeof value !== "undefined") {
                    // Only check if valid ma ga
                    //var elementName = angular.element(element[0]).attr('name');
                    //if (formCtrl[elementName].$valid) {
                    if (value.length >= 1 && value.match(/^[0-9a-zA-Z\_]+$/)) {
                        userService.existUserName(value, userId, function (isExist) {
                            modelCtrl.$setValidity('checkExistUsername', !isExist);
                        });

                    }
                }
                return value;
            };
            modelCtrl.$parsers.push(applyValidateFn);
        }
    }
}])
.directive('checkExistEmail', ['$parse', 'userService', function ($parse, userService) {

    return {
        require: ['^form', 'ngModel'],
        restrict: 'A',
        link: function ($scope, element, attrs, ctrls) {
            var userId;
            var formCtrl = ctrls[0];
            var modelCtrl = ctrls[1];
            attrs.$observe('checkExistEmail', function (value) {
                userId = value;
                // Check on first load
                applyValidateFn(angular.element(element[0]).val());
            });

            var applyValidateFn = function (value) {
                modelCtrl.$setValidity('checkExistEmail', true);
                if (value !== "" && typeof value !== "undefined") {
                    // Only check if valid ma ga
                    //var elementName = angular.element(element[0]).attr('name');
                    //if (formCtrl[elementName].$valid) {
                    if (value.length >= 1 && value.match(/^[0-9a-zA-Z\_]+$/)) {
                        userService.existEmail(value, userId, function (isExist) {
                            modelCtrl.$setValidity('checkExistEmail', !isExist);
                        });

                    }
                }
                return value;
            };
            modelCtrl.$parsers.push(applyValidateFn);
        }
    }
}]);
