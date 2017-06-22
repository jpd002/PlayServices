<?php

require_once("database.php");

function endPoint_game()
{
	if(!isset($_GET["id"]))
	{
		throw new Exception("id must be provided.");
	}

	$id = $_GET["id"];

	$database = new Database();
	$game = $database->GetGameFromId($id);
	if($game == null)
	{
		throw new Exception("Game with id '". $id . "' not found.");
	}

	return $game;
}

?>
