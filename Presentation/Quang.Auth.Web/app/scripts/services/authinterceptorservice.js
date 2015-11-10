'use strict';

/**
 * @ngdoc service
 * @name authclientApp.authintercepterservice
 * @description
 * # authintercepterservice
 * Service in the authclientApp.
 */
angular.module('authclientApp')
.service('authInterceptorService', ['$q', '$injector', '$location', '$window', 'localStorageService', function ($q, $injector, $location, $window, localStorageService) {
    // AngularJS will instantiate a singleton by calling "new" on this function
    var authInterceptorServiceFactory = {};
    var _request = function (config) {

        config.headers = config.headers || {};
        var authService = $injector.get('authService');
        //var authData = localStorageService.get('authorizationData');
        if (authService.authentication.isAuth) {
            //config.headers.Authorization = 'Bearer ' + authData.token;
            config.headers.Authorization = 'Bearer ' + authService.authentication.cscode + authService.token();
            config.headers.userName = authService.authentication.userName;
            config.headers.cscode = authService.authentication.cscode;
            //console.log(authService.token());
        }

        return config;
    }

    var _responseError = function (rejection) {
        if (rejection.status === 401) {
            var authService = $injector.get('authService');
            //var authData = localStorageService.get('authorizationData');
            if (authService.authentication.isAuth && authService.authentication.useRefreshTokens) {
                $location.path('/refresh');
                return $q.reject(rejection);
            } else {
                var $modal = $injector.get('$modal');
                var modalInstance = $modal.open({
                    templateUrl: 'views/unauthorized.html',
                    controller: 'UnauthorizedCtrl',
                    resolve: {
                        Message: function () {
                            return rejection.data.Message;
                        },
                        MessageDetail: function () {
                            return rejection.data.MessageDetail;
                        }
                    },
                    backdropClass: 'backdrop-unauthorized'
                });
                modalInstance.result.then(function (shouldLogin) {
                    if (shouldLogin) {
                        $location.path('/login');
                    } else {
                        $window.history.back();
                    }
                }, function () {
                    $location.path('/login');
                });
            }
        }
        return $q.reject(rejection);
    }

    authInterceptorServiceFactory.request = _request;
    authInterceptorServiceFactory.responseError = _responseError;

    return authInterceptorServiceFactory;

}]);
