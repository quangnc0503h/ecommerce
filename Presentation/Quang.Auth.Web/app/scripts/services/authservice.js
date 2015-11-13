'use strict';

/**
 * @ngdoc service
 * @name authclientApp.authservice
 * @description
 * # authservice
 * Service in the authclientApp.
 */
angular.module('authclientApp')
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

      var _token = '';

      var _saveRegistration = function (registration) {

          _logOut();

          return $http.post(ENV.urlApiAuth + 'api/account/register', registration).then(function (response) {
              return response;
          });

      };

      var logoutClient = function () {
          _authentication.isAuth = false;
          _authentication.userName = '';
          _authentication.roles = [];
          _authentication.useRefreshTokens = false;
          _token = '';
      }

      var _login = function (loginData) {

          var data = 'grant_type=password&username=' + loginData.userName + '&password=' + loginData.password;

          if (loginData.useRefreshTokens) {
              data = data + '&client_id=' + ngAuthSettings.clientId;
          }

          var deferred = $q.defer();

          $http.post(ENV.urlApiAuth + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
              var expiresTime = response[".expires"];
              if (loginData.useRefreshTokens) {
                  //localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, roles: response.roles.split(","), refreshToken: response.refresh_token, useRefreshTokens: true });
                  xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: loginData.userName, displayName: response.displayName, roles: response.roles.split(","), expires: expiresTime, refreshToken: response.refresh_token, useRefreshTokens: true })).then(function (o) {
                      if (o.success) {
                          xdLocalStorage.setItem('cscode', authServiceFactory.textEncode(response.userClientId), true).then(function () {
                              _authentication.isAuth = true;
                              _authentication.userName = loginData.userName;
                              _authentication.displayName = response.displayName;
                              _authentication.useRefreshTokens = loginData.useRefreshTokens;
                              _authentication.roles = response.roles.split(",");
                              _token = response.userClientId + response.access_token;
                              $rootScope.isAuthLoaded = true;
                              deferred.resolve(response);
                          }, function (err) {
                              logoutClient();
                              $rootScope.isAuthLoaded = true;
                              deferred.reject(err);
                          });
                      } else {
                          alert('Ops, could not store your data.');
                      }
                  }, function (err) {
                      logoutClient();
                      $rootScope.isAuthLoaded = true;
                      deferred.reject(err);
                  });
              }
              else {
                  //localStorageService.set('authorizationData', { token: response.access_token, userName: loginData.userName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false });
                  xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: loginData.userName, displayName: response.displayName, roles: response.roles.split(","), expires: expiresTime, refreshToken: '', useRefreshTokens: false })).then(function (o) {
                      if (o.success) {
                          xdLocalStorage.setItem('cscode', authServiceFactory.textEncode(response.userClientId), true).then(function () {
                              _authentication.isAuth = true;
                              _authentication.userName = loginData.userName;
                              _authentication.displayName = response.displayName;
                              _authentication.useRefreshTokens = loginData.useRefreshTokens;
                              _authentication.roles = response.roles.split(",");
                              _token = response.userClientId + response.access_token;
                              $rootScope.isAuthLoaded = true;
                              deferred.resolve(response);
                          }, function (err) {
                              logoutClient();
                              $rootScope.isAuthLoaded = true;
                              deferred.reject(err);
                          });
                      } else {
                          alert('Ops, could not store your data.');
                      }
                  }, function (err) {
                      logoutClient();
                      $rootScope.isAuthLoaded = true;
                      deferred.reject(err);
                  });
              }
          }).error(function (err, status) {
              logoutClient();
              $rootScope.isAuthLoaded = true;
              deferred.reject(err);
          });
          $rootScope.isAuthLoaded = false;

          return deferred.promise;
      };

      var _logOut = function () {
          var deferred = $q.defer();
          $http.post(ENV.urlApiAuth + 'api/Account/Logout', {}).success(function () {
              //localStorageService.remove('authorizationData');
              xdLocalStorage.removeItem('xd.authorization').then(function () {
                  xdLocalStorage.removeItem('cscode', true).then(function () {
                      logoutClient();
                      $rootScope.isAuthLoaded = true;
                      deferred.resolve();
                  }, function (err) {
                      logoutClient();
                      $rootScope.isAuthLoaded = true;
                      deferred.reject(err);
                  });
              }, function (err) {
                  logoutClient();
                  $rootScope.isAuthLoaded = true;
                  deferred.reject(err);
              });
              $rootScope.isAuthLoaded = false;
          }).error(function (err, status) {
              //localStorageService.remove('authorizationData');
              xdLocalStorage.removeItem('xd.authorization').then(function () {
                  xdLocalStorage.removeItem('cscode', true).then(function () {
                      logoutClient();
                      $rootScope.isAuthLoaded = true;
                      deferred.reject(err);
                  }, function (err) {
                      logoutClient();
                      $rootScope.isAuthLoaded = true;
                      deferred.reject(err);
                  });
              }, function (err) {
                  logoutClient();
                  $rootScope.isAuthLoaded = true;
                  deferred.reject(err);
              });
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
                      var expiresTime = response[".expires"];
                      //localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, displayName: response.displayName, refreshToken: response.refresh_token, useRefreshTokens: true });
                      xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), expires: expiresTime, refreshToken: response.refresh_token, useRefreshTokens: true })).then(function (o) {
                          if (o.success) {
                              xdLocalStorage.setItem('cscode', authServiceFactory.textEncode(response.userClientId), true).then(function () {
                                  _authentication.isAuth = true;
                                  _authentication.userName = response.userName;
                                  _authentication.displayName = response.displayName;
                                  _authentication.useRefreshTokens = true;
                                  _authentication.roles = response.roles.split(",");
                                  _token = response.userClientId + response.access_token;
                                  $rootScope.isAuthLoaded = true;
                                  deferred.resolve(response);
                              });
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
              var expiresTime = response[".expires"];
              //localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false });
              xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), expires: expiresTime, refreshToken: '', useRefreshTokens: false })).then(function (o) {
                  if (o.success) {
                      xdLocalStorage.setItem('cscode', response.userClientId, true).then(function () {
                          _authentication.isAuth = true;
                          _authentication.userName = response.userName;
                          _authentication.displayName = response.displayName;
                          _authentication.useRefreshTokens = false;
                          _authentication.roles = response.roles.split(",");
                          _token = response.userClientId + response.access_token;
                          $rootScope.isAuthLoaded = true;
                          deferred.resolve(response);
                      });
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
              var expiresTime = response[".expires"];
              //localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), refreshToken: '', useRefreshTokens: false });
              xdLocalStorage.setItem('xd.authorization', JSON.stringify({ token: response.access_token, userName: response.userName, displayName: response.displayName, roles: response.roles.split(","), expires: expiresTime, refreshToken: '', useRefreshTokens: false })).then(function (o) {
                  if (o.success) {
                      xdLocalStorage.setItem('cscode', response.userClientId, true).then(function () {
                          _authentication.isAuth = true;
                          _authentication.userName = response.userName;
                          _authentication.displayName = response.displayName;
                          _authentication.useRefreshTokens = false;
                          _authentication.roles = response.roles.split(",");
                          _token = response.userClientId + response.access_token;
                          $rootScope.isAuthLoaded = true;
                          deferred.resolve(response);
                      });
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

      var _originCharArr = new Array('a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F', 'g', 'G', 'h', 'H', 'i', 'I', 'j', 'J', 'k', 'K', 'l', 'L', 'm', 'M', 'n', 'N', 'o', 'O', 'p', 'P', 'q', 'Q', 'r', 'R', 's', 'S', 't', 'T', 'u', 'U', 'v', 'V', 'w', 'W', 'x', 'X', 'y', 'Y', 'z', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '_', '-');
      var _complexCharArr = new Array('q1', 'Q1', 'wq', 'Wq', 'ea', 'Ea', 'rz', 'Rz', 't2', 'T2', 'yw', 'Yw', 'us', 'Us', 'ix', 'Ix', 'o3', 'O3', 'pe', 'Pe', 'ad', 'Ad', 'sc', 'Sc', 'd4', 'D4', 'fr', 'Fr', 'gf', 'Gf', 'hv', 'Hv', 'j5', 'J5', 'kt', 'Kt', 'lg', 'Lg', 'zb', 'Zb', 'x6', 'X6', 'cy', 'Cy', 'vh', 'Vh', 'bn', 'Bn', 'n7', 'N7', 'mu', 'Mu', '1j', '2m', '38', '4i', '5k', '69', '7o', '8l', '90', '0p', '01', '02');

      var _textEncode = function (text) {
          text = new String(text);
          var str = '';
          for (var i = 0; i < text.length; i++) {
              var key = null;
              for (var j = 0; j < _originCharArr.length; j++) {
                  if (text[i] == _originCharArr[j]) {
                      key = j;
                      break;
                  }
              }
              var rnd = Math.floor(Math.random() * (_originCharArr.length));
              var tmp = _complexCharArr[key];
              if (i % 2 == 0) {
                  tmp = _originCharArr[rnd] + tmp;
              } else {
                  tmp = tmp + _originCharArr[rnd];
              }
              str += tmp;
          }
          return str;
      }
      var _textDecode = function (text) {
          text = new String(text);
          var len = text.length / 3;
          var str = new String();
          for (var i = 0; i < len; i++) {
              var key = null;
              var item = text.substr(i * 3, 3);
              if (i % 2 == 0) {
                  item = item.substr(1, 2);
              } else {
                  item = item.substr(0, 2);
              }
              for (var j = 0; j < _complexCharArr.length; j++) {
                  if (item == _complexCharArr[j]) {
                      key = j;
                      break;
                  }
              }

              str += _originCharArr[key];
          }
          return str;
      }

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
      authServiceFactory.textEncode = _textEncode;
      authServiceFactory.textDecode = _textDecode;

      return authServiceFactory;
  }]);