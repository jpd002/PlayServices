<?php

declare(strict_types = 1);
require_once(__DIR__ . "/../endpoint_builds.php");

use PHPUnit\Framework\TestCase;

final class Test_Builds extends TestCase
{
	public function testSimpleGet()
	{
		$buildInfo = 
			[
				"commitHash" => "01234567",
				"commitMessage" => "My Build",
				"hasBuild" => true,
				"timestamp" => (new DateTime())->getTimestamp()
			];
		
		$buildsDao = $this->createMock("BuildsDao");
		$buildsDao->expects($this->once())
		    ->method("getBuildInfo")
		    ->will($this->returnValue($buildInfo));
		
		$githubDao = $this->createMock("GithubDao");
		
		$endpoint = Endpoint_Builds::createWithDao($buildsDao, $githubDao);
		$buildInfo = $endpoint->ExecuteGet();
		$this->assertEquals("My Build", $buildInfo["commitMessage"]);
	}
	
	public function testOutdatedGet()
	{
		$outdatedBuildInfo = 
			[
				"commitHash" => "01234567",
				"commitMessage" => "Outdated",
				"hasBuild" => true,
				"timestamp" => (new DateTime())->getTimestamp() - 3601
			];
		
		$newBuildInfo = 
			[
				"commitHash" => "01234567",
				"commitMessage" => "New",
				"hasBuild" => true,
				"timestamp" => (new DateTime())->getTimestamp()
			];
		
		$buildsDao = $this->createMock("BuildsDao");
		$buildsDao->expects($this->once())
		    ->method("getBuildInfo")
		    ->will($this->returnValue($outdatedBuildInfo));
		$buildsDao->expects($this->once())
		    ->method("setBuildInfo");
		
		$githubDao = $this->createMock("GithubDao");
		$githubDao->expects($this->once())
		    ->method("getTopCommitInfo")
		    ->will($this->returnValue($newBuildInfo));
		
		$endpoint = Endpoint_Builds::createWithDao($buildsDao, $githubDao);
		$buildInfo = $endpoint->ExecuteGet();
		$this->assertEquals("New", $buildInfo["commitMessage"]);
	}
};

?>
