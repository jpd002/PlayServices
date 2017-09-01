<?php

require_once("database.php");
require_once("endpoint.php");

class Endpoint_Game extends Endpoint
{
	function executeGet()
	{
		$id = $this->getParam($_GET, "id");
		
		$database = new Database();
		$game = $database->GetGameFromId($id);
		if($game == null)
		{
			throw new Exception("Game with id '". $id . "' not found.");
		}

		return $game;
	}
}

?>
