'use strict'

angular.module('playApp.compatibilityView', ['ngRoute'])

.config(
	['$routeProvider', 
	function($routeProvider)
	{
		$routeProvider.when('/compatibilityView',
			{
				templateUrl: 'compatibilityView.html',
				controller:  'compatibilityViewController'
			}
		);
	}]
)

.controller('compatibilityViewController', 
	['$scope', '$http', 'playApp.apiBaseUrl',
	function($scope, $http, apiBaseUrl)
	{
		$scope.gameId = 'SLES_502.82';
		$http(
			{
				method: "GET",
				url: apiBaseUrl + '?endpoint=compatibility&gameId=' + $scope.gameId
			}
		).then(
			function mySuccess(response)
			{
				$scope.compatibilityReports = response.data;
			},
			function myError(response)
			{
				//$scope.myWelcome = response.statusText;
			}
		);
	}]
)

;
