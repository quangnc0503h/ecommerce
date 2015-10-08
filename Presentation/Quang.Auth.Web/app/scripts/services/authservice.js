'use strict';

/**
 * @ngdoc service
 * @name authclientApp.authservice
 * @description
 * # authservice
 * Service in the authclientApp.
 */
angular.module('quangauthwebApp')
  .service('authService', ['$rootScope', '$http', '$q', 'localStorageService', 'ngAuthSettings', 'ENV', 'xdLocalStorage', function ($rootScope, $http, $q, localStorageService, ngAuthSettings, ENV, xdLocalStorage) {
      
      var authServiceFactory = {};

      var _authentication = {
          isAuth: false,
          userName: '',
          roles: [],
          useRefreshTokens: false
      };

      var _externalAuthData = {
          provider: '',
          userName: '',
          externalAccessToken: ''
      };
      //$rootScope.isAuthLoaded = true;
      var _token = '';

      var _saveRegistration = function (registration) {

          _logOut();

          return $http.post(ENV.urlApiAuth + 'api/account/register', registration).then(function (response) {
              return response;
          });

      };

      var _login = function (loginData) {

          var data = 'grant_type=password&username=' + loginData.userName + '&password=' + loginData.password;

          if (loginData.useRefreshTokens) {
              data = data + '&client_id=' + ngAuthSettings.clientId;
          }

          var deferred = $q.defer();
       //   console.log(data);
          $http.post(ENV.urlApiAuth + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

              if (loginData.useRefreshTokens) {
                  //localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, roles: response.roles.split(","), refreshToken: response.refresh_token, useRefreshTokens: true });
                  xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: loginData.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: response.refresh_token, useRefreshTokens: true })).then(function (o) {
                      if (o.success) {
                          _authentication.isAuth = true;
                          _authentication.userName = loginData.userName;
                          _authentication.displayName = response.displayName;
                          _authentication.useRefreshTokens = loginData.useRefreshTokens;
                          _authentication.roles = response.roles.split(",");
                          _token = response.access_token;
                          $rootScope.isAuthLoaded = true;
                          deferred.resolve(response);
                      } else {
                          alert('Ops, could not store your data.');
                      }
                  });
              }
              else {
                  //localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false });
                  xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: loginData.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: response.refresh_token, useRefreshTokens: true })).then(function (o) {
                      if (o.success) {
                          _authentication.isAuth = true;
                          _authentication.userName = loginData.userName;
                          _authentication.displayName = response.displayName;
                          _authentication.useRefreshTokens = loginData.useRefreshTokens;
                          _authentication.roles = response.roles.split(",");
                          _token = response.access_token;
                          $rootScope.isAuthLoaded = true;
                          deferred.resolve(response);
                      } else {
                          alert('Ops, could not store your data.');
                      }
                  });
              }
          }).error(function (err, status) {
              _logOut();
              deferred.reject(err);
          });
          $rootScope.isAuthLoaded = false;

          return deferred.promise;
      };

      var _logOut = function () {
          var deferred = $q.defer();
          //localStorageService.remove('authorizationData');
          xdLocalStorage.removeItem('xd.authorization').then(function () {
              _authentication.isAuth = false;
              _authentication.userName = '';
              _authentication.roles = [];
              _authentication.useRefreshTokens = false;
              $rootScope.isAuthLoaded = true;
              deferred.resolve();
          });
          $rootScope.isAuthLoaded = false;
          return deferred.promise;
      };

      var _fillAuthData = function (authData, token) {
          //var authData = localStorageService.get('authorizationData');
          if (authData) {
              _authentication.isAuth = true;
              _authentication.userName = authData.userName;
              _authentication.displayName = authData.displayName;
              _authentication.useRefreshTokens = authData.useRefreshTokens;
              _authentication.roles = authData.roles;
              _token = token;
          }
      };

      var _refreshToken = function () {
          var deferred = $q.defer();

          //var authData = localStorageService.get('authorizationData');
          xdLocalStorage.getItem('xd.authorization').then(function (data) {
              var authData;
              if (data.value) {
                  try {
                      authData = JSON.parse(data.value);
                  } catch (err) {
                      //not our message, can ignore
                  }
              }
              if (authData && authData.token && authData.useRefreshTokens) {
                  var data = 'grant_type=refresh_token&refresh_token=' + authData.refreshToken + '&client_id=' + ngAuthSettings.clientId;

                  localStorageService.remove('authorizationData');

                  $http.post(ENV.urlApiAuth + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {

                      //localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, displayName: response.displayName, refreshToken: response.refresh_token, useRefreshTokens: true });
                      xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: response.userName, displayName: response.displayName, refreshToken: response.refresh_token, useRefreshTokens: true })).then(function (o) {
                          if (o.success) {
                              _authentication.isAuth = true;
                              _authentication.userName = response.userName;
                              _authentication.displayName = response.displayName;
                              _authentication.useRefreshTokens = true;
                              _authentication.roles = response.roles.split(",");
                              _token = response.access_token;
                              $rootScope.isAuthLoaded = true;
                              deferred.resolve(response);
                          } else {
                              alert('Ops, could not store your data.');
                          }
                      });
                  }).error(function (err, status) {
                      _logOut();
                      deferred.reject(err);
                  });
              }
          }, function (err) {
              deferred.reject(err);
          });

          return deferred.promise;
      };

      var _obtainAccessToken = function (externalData) {
          
          var deferred = $q.defer();

          $http.get(ENV.urlApiAuth + 'api/account/ObtainLocalAccessToken', { params: { provider: externalData.provider, externalAccessToken: externalData.externalAccessToken } }).success(function (response) {
              
              //localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false });
              xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false })).then(function (o) {
                  if (o.success) {
                      _authentication.isAuth = true;
                      _authentication.userName = response.userName;
                      _authentication.displayName = response.displayName;
                      _authentication.useRefreshTokens = false;
                      _authentication.roles = response.roles.split(",");
                      _token = response.access_token;
                      $rootScope.isAuthLoaded = true;
                      deferred.resolve(response);
                  } else {
                      alert('Ops, could not store your data.');
                  }
              });
          }).error(function (err, status) {
              _logOut();
              deferred.reject(err);
          });

          return deferred.promise;

      };

      var _registerExternal = function (registerExternalData) {

          var deferred = $q.defer();

          $http.post(ENV.urlApiAuth + 'api/account/registerexternal', registerExternalData).success(function (response) {

              //localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false });
              xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false })).then(function (o) {
                  if (o.success) {
                      _authentication.isAuth = true;
                      _authentication.userName = response.userName;
                      _authentication.displayName = response.displayName;
                      _authentication.useRefreshTokens = false;
                      _authentication.roles = response.roles.split(",");
                      _token = response.access_token;
                      $rootScope.isAuthLoaded = true;
                      deferred.resolve(response);
                  } else {
                      alert('Ops, could not store your data.');
                  }
              });
          }).error(function (err, status) {
              _logOut();
              deferred.reject(err);
          });
          
          return deferred.promise;

      };

      authServiceFactory.saveRegistration = _saveRegistration;
      authServiceFactory.login = _login;
      authServiceFactory.logOut = _logOut;
      authServiceFactory.fillAuthData = _fillAuthData;
      authServiceFactory.authentication = _authentication;
      authServiceFactory.refreshToken = _refreshToken;

      authServiceFactory.obtainAccessToken = _obtainAccessToken;
      authServiceFactory.externalAuthData = _externalAuthData;
      authServiceFactory.registerExternal = _registerExternal;
      authServiceFactory.token = function () { return _token; }

      return authServiceFactory;
  }]);
