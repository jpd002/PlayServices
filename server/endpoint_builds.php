<?php

require_once("endpoint.php");
require_once("config.php");
require_once(__DIR__ . '/vendor/autoload.php');

class Endpoint_Builds extends Endpoint
{
	function getTopCommitInfo()
	{
		global $gh_apitoken;
		
		$client = new \Github\Client();
		$client->authenticate($gh_apitoken, "", Github\Client::AUTH_URL_TOKEN);

		$branch = $client->api("repo")->branches("jpd002", "Play-", "master");
		$commit = $branch["commit"];
		$commitHash = $commit["sha"];
		$commitMessage = $commit["commit"]["message"];
		$commitShortHash = substr($commitHash, 0, 8);
		$commitStatus = $client->api("repo")->statuses()->combined("jpd002", "Play-", $commitHash);
		$hasBuild = $commitStatus["state"] === "success";
		
		return 
			[
				"commitHash" => $commitShortHash,
				"commitMessage" => $commitMessage,
				"hasBuild" => $hasBuild,
				"timestamp" => (new DateTime())->getTimestamp()
			];
	}
	
	function createDbClient()
	{
		global $builds_aws_access_key;
		global $builds_aws_access_secret;
		global $builds_aws_region;

		return 
			Aws\DynamoDb\DynamoDbClient::factory(
				array(
					"credentials" => array(
										"key"    => $builds_aws_access_key,
										"secret" => $builds_aws_access_secret,
									),
					"region"      => $builds_aws_region,
					"version"     => "2012-08-10"
				)
			);
	}
	
	function updateBuildInfo($dbClient, $buildInfo)
	{
		$marshaler = new Aws\DynamoDb\Marshaler();
		$item = $marshaler->marshalItem(
			[
				"commit" => "0",
				"buildInfo" => $buildInfo
			]
		);
		$params = 
			[
				"TableName" => "play_buildinfo",
				"Item" => $item
			];
		$dbClient->putItem($params);
	}
	
	function readBuildInfo($dbClient)
	{
		$marshaler = new Aws\DynamoDb\Marshaler();
		$params = 
			[
				"TableName" => "play_buildinfo"
			];
		$response = $dbClient->scan($params);
		$items = $response["Items"];
		if(sizeof($items) == 0) return null;
		$item = $response["Items"][0];
		$value = $marshaler->unmarshalItem($item);
		return $value["buildInfo"];
	}
	
	function executeGet()
	{
		//TODO: Check if it's up to date
		$dbClient = $this->createDbClient();
		$buildInfo = $this->readBuildInfo($dbClient);
		$buildInfoTimestamp = $buildInfo["timestamp"];
		$nowTimestamp = (new DateTime())->getTimestamp();
		$buildInfoAge = $nowTimestamp - $buildInfoTimestamp;
		if($buildInfoAge > 3600)  //3600 seconds -> 1 hour
		{
			//Update commit info
			$buildInfo = $this->getTopCommitInfo();
			$this->updateBuildInfo($dbClient, $buildInfo);
		}
		return $buildInfo;
	}
}

?>
