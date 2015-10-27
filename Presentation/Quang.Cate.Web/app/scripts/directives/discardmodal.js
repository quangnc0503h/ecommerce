(function (window, angular, undefined) {    
    'use strict';

    var app = angular.module('quangcatewebApp');
    app.directive('discardModal', ['$rootScope', '$modalStack',
    function ($rootScope, $modalStack) {
        return {
            restrict: 'A',
            link: function () {
                /**
                * If you are using ui-router, use $stateChangeStart method otherwise use $locationChangeStart
                 * StateChangeStart will trigger as soon as the user clicks browser back button or keyboard backspace and modal will be removed from modal stack
                 */
                $rootScope.$on('$stateChangeStart', function (event) {
                    var top = $modalStack.getTop();
                    if (top) {
                        $modalStack.dismiss(top.key);
                    }
                });
            }
        };
    }
    ]);
})(window, window.angular);