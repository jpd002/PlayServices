'use strict'

angular.module('playApp.compatibilityAdd', ['ngRoute'])

.config(
	['$routeProvider', 
	function($routeProvider)
	{
		$routeProvider.when('/compatibilityAdd',
			{
				templateUrl: 'compatibilityAdd.html',
				controller:  'compatibilityAddController'
			}
		);
	}]
)

.controller('compatibilityAddController',
	['$scope', '$http', 'playApp.apiBaseUrl',
	function($scope, $http, apiBaseUrl)
	{
		$scope.postCompat =
			function()
			{
				$http.post(
						apiBaseUrl + '?endpoint=compatibility', 
						{
							gameId: $scope.gameId,
							rating: $scope.rating,
							deviceInfo: 
							{
								osName: 'web'
							}
						}
					)
					.then(
						function()
						{
							
						},
						function(response)
						{
							alert(response.data.error.description);
						}
					);
			};
	}]
)

;
