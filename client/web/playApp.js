'use strict'

angular.module('playApp', 
	[
		'ngRoute',
		'playApp.compatibilityAdd',
		'playApp.compatibilityView'
	]
)

.config(
	['$locationProvider', '$routeProvider', 
	function($locationProvider, $routeProvider)
	{
		$locationProvider.hashPrefix('!');
		$routeProvider.otherwise({ redirectTo: '/compatibilityView'});
	}
])

.constant('playApp.apiBaseUrl', 'http://localhost/playservices/server/api.php')

;
