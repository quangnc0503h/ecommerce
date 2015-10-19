'use strict';

/**
 * @ngdoc service
 * @name danhmucApp.tockensmanagerService
 * @description
 * # tockensmanagerService
 * Service in the danhmucApp.
 */
angular.module('quangcatewebApp')
  .service('tockensmanagerService', ['$http', 'ENV', function ($http, ENV) {
      
      var tokenManagerServiceFactory = {};

      var _getRefreshTokens = function () {

          return $http.get(ENV.urlApiAuth + 'api/refreshtokens').then(function (results) {
              return results;
          });
      };

      var _deleteRefreshTokens = function (tokenid) {

          return $http.delete(ENV.urlApiAuth + 'api/refreshtokens/?tokenid=' + tokenid).then(function (results) {
              return results;
          });
      };

      tokenManagerServiceFactory.deleteRefreshTokens = _deleteRefreshTokens;
      tokenManagerServiceFactory.getRefreshTokens = _getRefreshTokens;

      return tokenManagerServiceFactory;
  }]);
