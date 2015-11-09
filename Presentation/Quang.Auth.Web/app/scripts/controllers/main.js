'use strict';

/**
 * @ngdoc function
 * @name authclientApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the authclientApp
 */
angular.module('authclientApp')
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
