CREATE TABLE `ps_compatibility` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `gameId` varchar(20) NOT NULL,
  `rating` int(11) NOT NULL,
  `deviceInfo` json NOT NULL,
  PRIMARY KEY (`id`)
) DEFAULT CHARSET=utf8
