<?php

require_once("database.php");
require_once("endpoint.php");

class Endpoint_Compatibility extends Endpoint
{
	function executeGet()
	{
		$gameId = $this->getParam($_GET, "gameId");
		
		$database = new Database();
		$result = $database->GetCompatibility($gameId);
		
		return $result;
	}

	function executePost()
	{
		$rawParams = file_get_contents("php://input");
		$params = json_decode($rawParams, TRUE);
		
		$gameId     = $this->getParam($params, "gameId");
		$rating     = $this->getParam($params, "rating");
		$deviceInfo = $this->getParam($params, "deviceInfo");
		
		$database = new Database();
		$database->InsertCompatibility($gameId, $rating, json_encode($deviceInfo));
		
		return array("result" => "ok");
	}
}

?>
