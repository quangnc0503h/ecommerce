'use strict';

/**
 * @ngdoc function
 * @name quangauthwebApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the quangauthwebApp
 */
angular.module('quangauthwebApp')
   .controller('MainCtrl', ['$scope', '$location', 'authService', function ($scope, $location, authService) {
       //alert(12);
       if (authService.authentication.isAuth) {
           //TODO
       }

       $scope.logOut = function () {
           authService.logOut();
           $location.path('/home');
       }
   }]);
