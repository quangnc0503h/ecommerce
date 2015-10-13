'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:LoginCtrl
 * @description
 * # LoginCtrl
 * Controller of the authclientApp
 */
angular.module('quangauthwebApp').
controller("LoginCtrl", ["$scope", "$location", "$routeParams", "authService", "ngAuthSettings", "ENV", "xdLocalStorage", function (a, b, c, d, e, f, g) {
    
    //console.log(xdLocalStorage.getItem("url_3438512491993606", !0));
    g.getItem("url_3438512491993606").then(function (response) {
        console.log(response.value);
    });
    var h; c.returnUrl && g.getItem("url_3438512491993606", !0).then(function (a) {
        
        console.log(a);
        h = a.value, d.authentication.isAuth ? g.removeItem("url_" + c.returnUrl, !0).then(function () {
            window.location.href = decodeURIComponent(h)
        }, function (a) {
            window.location.href = decodeURIComponent(h)
        }) : g.setItem("current_back_url", c.returnUrl).then(function (a) { })
    }), !c.returnUrl && d.authentication.isAuth && b.path("/"),
    a.loginData = { userName: "", password: "", useRefreshTokens: !1 },
    a.message = "",
    a.login = function () {
        a.isDisabledBtnLogin = !0, d.login(a.loginData).then(function (d) {
            h ? g.removeItem("url_" + c.returnUrl, !0).then(function () {
                window.location.href = decodeURIComponent(h)
            }, function (a) { window.location.href = decodeURIComponent(h) }) : b.path("/"), a.isDisabledBtnLogin = !1
        }, function (b) { a.message = b.error_description, a.isDisabledBtnLogin = !1 })
    },
    a.authExternalProvider = function (b) {
        var c = location.protocol + "//" + location.host + "/authcomplete.html", d = f.urlApiAuth + "api/Account/ExternalLogin?provider=" + b + "&response_type=token&client_id=" + e.clientId + "&redirect_uri=" + c; window.$windowScope = a; window.open(d, "Authenticate Account", "location=0,status=0,width=600,height=750")
    },
    a.authCompletedCB = function (e) {
        a.$apply(function () { if ("False" == e.haslocalaccount) d.logOut(), d.externalAuthData = { provider: e.provider, userName: e.external_user_name, email: e.external_email, externalAccessToken: e.external_access_token }, b.path("/associate"); else { var f = { provider: e.provider, externalAccessToken: e.external_access_token }; d.obtainAccessToken(f).then(function (a) { g.getItem("current_back_url").then(function (a) { a.value ? g.getItem("url_" + a.value, !0).then(function (a) { g.removeItem("url_" + c.returnUrl, !0).then(function () { window.location.href = decodeURIComponent(a.value) }, function (b) { window.location.href = decodeURIComponent(a.value) }) }) : b.path("/") }, function (a) { b.path("/") }) }, function (b) { a.message = b.error_description }) } })
    },
    window.setTimeout(function () { $("#userName").focus() }, 100)
}]).
/*.controller('LoginCtrl', ['$scope', '$location', "$routeParams", 'authService', 'ngAuthSettings', 'ENV', 'xdLocalStorage', function ($scope, $location, $routeParams, authService, ngAuthSettings, ENV, xdLocalStorage) {
      var current_back_url;
      console.log($routeParams.returnUrl);
      if ($routeParams.returnUrl) {
          xdLocalStorage.getItem("url_" + $routeParams.returnUrl, true).then(function (a) {
              //console.log(a);
              current_back_url = a.value;
              if (authService.authentication.isAuth) {
                  xdLocalStorage.removeItem("url_" + $location.returnUrl, true).then(function () {
                      window.location.href = decodeURIComponent(current_back_url)
                  },
		        function (a) {
		            window.location.href = decodeURIComponent(current_back_url);
		        });
              } else {
                  xdLocalStorage.setItem("current_back_url", c.returnUrl).then(function (a) { });
              }
          });
      }
      else {
          if (authService.authentication.isAuth) {
              $location.path('/');
          }
      }
      
      $scope.loginData = {
          userName: '',
          password: '',
          useRefreshTokens: false
      };

      $scope.message = '';

      $scope.login = function () {
          $scope.isDisabledBtnLogin = true;
          authService.login($scope.loginData).then(function (response) {
              console.log(response);
              $location.path('/');
          },
          function (err) {
              $scope.message = err.error_description;
          });
      };

      $scope.authExternalProvider = function (provider) {

          var redirectUri = location.protocol + '//' + location.host + '/authComplete.html';

          var externalProviderUrl = ENV.urlApiAuth + 'api/Account/ExternalLogin?provider=' + provider
                                                                      + '&response_type=token&client_id=' + ngAuthSettings.clientId
                                                                      + '&redirect_uri=' + redirectUri;
          window.$windowScope = $scope;

          var oauthWindow = window.open(externalProviderUrl, 'Authenticate Account', 'location=0,status=0,width=600,height=750');
      };

      $scope.authCompletedCB = function (fragment) {

          $scope.$apply(function () {

              if (fragment.haslocalaccount == 'False') {

                  authService.logOut();

                  authService.externalAuthData = {
                      provider: fragment.provider,
                      userName: fragment.external_user_name,
                      email: fragment.external_email,
                      externalAccessToken: fragment.external_access_token
                  };

                  $location.path('/associate');

              }
              else {
                  //Obtain access token and redirect to orders
                  var externalData = { provider: fragment.provider, externalAccessToken: fragment.external_access_token };
                  authService.obtainAccessToken(externalData).then(function (response) {
                      //$location.path('/orders');
                      $location.path('/');

                  },
               function (err) {
                   $scope.message = err.error_description;
               });
              }

          });
      }
  }])
  .*/
controller('LogoutCtrl', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
      authService.logOut().then(function () {
          $location.path('/');
      });
  }]);
