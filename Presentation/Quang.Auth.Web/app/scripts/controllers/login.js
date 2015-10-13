'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:LoginCtrl
 * @description
 * # LoginCtrl
 * Controller of the authclientApp
 */
angular.module('quangauthwebApp').
controller("LoginCtrl", ["$scope", "$location", "$routeParams", "authService", "ngAuthSettings", "ENV", "xdLocalStorage", function ($scope, $location, $routeParams, authService, ngAuthSettings, ENV, xdLocalStorage) {
    var current_back_url;
    $routeParams.returnUrl && xdLocalStorage.getItem("url_" + $routeParams.returnUrl).then(function (response) {
        current_back_url = response.value;
        authService.authentication.isAuth ? xdLocalStorage.removeItem("url_" + $routeParams.returnUrl).then(function () {
            window.location.href = decodeURIComponent(current_back_url);
        }, function () {
            window.location.href = decodeURIComponent(current_back_url);
        }) : xdLocalStorage.setItem("current_back_url", $routeParams.returnUrl).then(function () { });
    });
    !$routeParams.returnUrl && authService.authentication.isAuth && $location.path("/");
    $scope.loginData = {
        userName: "",
        password: "",
        useRefreshTokens: false
    };
    $scope.message = "";
    $scope.login = function () {
        $scope.isDisabledBtnLogin = true;
        authService.login($scope.loginData).then(function () {
            current_back_url ? xdLocalStorage.removeItem("url_" + $routeParams.returnUrl).then(function () {
                window.location.href = decodeURIComponent(current_back_url);
            }, function () {
                window.location.href = decodeURIComponent(current_back_url);
            }) : $location.path("/");
            $scope.isDisabledBtnLogin = false;
        },
            function (err) {
                $scope.message = err.error_description;
                $scope.isDisabledBtnLogin = false;
            });
    };
    $scope.authExternalProvider = function (provider) {
        var redirectUri = location.protocol + "//" + location.host + "/authcomplete.html";
        var externalProviderUrl = ENV.urlApiAuth + "api/Account/ExternalLogin?provider=" + provider + "&response_type=token&client_id=" + ngAuthSettings.clientId + "&redirect_uri=" + redirectUri;
        window.$windowScope = $scope;
        window.open(externalProviderUrl, "Authenticate Account", "location=0,status=0,width=600,height=750");
    };
    $scope.authCompletedCB = function (fragment) {
        $scope.$apply(function () {
            if ("False" == fragment.haslocalaccount) {
                authService.logOut();
                authService.externalAuthData = {
                    provider: fragment.provider,
                    userName: fragment.external_user_name,
                    email: fragment.external_email,
                    externalAccessToken: fragment.external_access_token
                };
                $location.path("/associate");
            } else {
                var externalData = { provider: fragment.provider, externalAccessToken: fragment.external_access_token };
                authService.obtainAccessToken(externalData).then(function (ret) {
                    xdLocalStorage.getItem("current_back_url").then(function (response) {
                        response.value ? xdLocalStorage.getItem("url_" + response.value).then(function (res) {
                            xdLocalStorage.removeItem("url_" + $routeParams.returnUrl).then(function () {
                                window.location.href = decodeURIComponent(res.value);
                            }, function () { window.location.href = decodeURIComponent(ret.value); });
                        }) : $location.path("/");
                    }, function () { $location.path("/"); });
                }, function (err) { $scope.message = err.error_description; });
            }
        });
    };
    window.setTimeout(function () {
        $("#userName").focus();
    }, 100);
}]).
controller('LogoutCtrl', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
    authService.logOut().then(function () {
        $location.path('/');
    });
}]);
