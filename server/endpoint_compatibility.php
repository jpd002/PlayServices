<?php

require_once("endpoint.php");
require_once(__DIR__ . "/vendor/autoload.php");

use Aws\S3\S3Client;

class Endpoint_Compatibility extends Endpoint
{
	function executeGet()
	{
		global $compat_aws_region;
		global $compat_s3_bucket_name;
		
		//If no gameId is provided, return summary
		$s3 = new S3Client([
			"version"     => "latest",
			"region"      => $compat_aws_region,
			"credentials" => false
		]);
		$result = $s3->getObject([
			"Bucket" => $compat_s3_bucket_name,
			"Key"    => "compat_summary.json"
		]);
		$result = json_decode($result["Body"]);
		return $result;
	}
}

?>
