'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:LoginCtrl
 * @description
 * # LoginCtrl
 * Controller of the authclientApp
 */
angular.module('quangauthwebApp')
  .controller('LoginCtrl', ['$scope', '$location', 'authService', 'ngAuthSettings', 'ENV', function ($scope, $location, authService, ngAuthSettings, ENV) {
      if (authService.authentication.isAuth) {
          $location.path('/');
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
  .controller('LogoutCtrl', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
      authService.logOut().then(function () {
          $location.path('/');
      });
  }]);
