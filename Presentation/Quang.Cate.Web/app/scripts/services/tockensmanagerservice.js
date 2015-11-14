'use strict';

/**
 * @ngdoc service
 * @name danhmucApp.tockensmanagerService
 * @description
 * # tockensmanagerService
 * Service in the danhmucApp.
 */
angular.module('quangcatewebApp')
 .service('tokensmanagerService', ['$http', 'ENV', function ($http, ENV) {

     var tokenmanagerServiceFactory = {};

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

     tokenmanagerServiceFactory.deleteRefreshTokens = _deleteRefreshTokens;
     tokenmanagerServiceFactory.getRefreshTokens = _getRefreshTokens;

     return tokenmanagerServiceFactory;
 }]);
