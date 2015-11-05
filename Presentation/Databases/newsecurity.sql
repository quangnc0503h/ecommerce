/*
SQLyog Community v12.09 (64 bit)
MySQL - 10.0.17-MariaDB-wsrep : Database - Security
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`Security` /*!40100 DEFAULT CHARACTER SET utf8 */;

USE `Security`;

/*Table structure for table `Clients` */

DROP TABLE IF EXISTS `Clients`;

CREATE TABLE `Clients` (
  `Id` varchar(200) NOT NULL,
  `Secret` varchar(500) DEFAULT NULL,
  `Name` varchar(200) NOT NULL,
  `ApplicationType` int(11) NOT NULL,
  `Active` int(11) NOT NULL,
  `RefreshTokenLifeTime` int(11) NOT NULL,
  `AllowedOrigin` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `Devices` */

DROP TABLE IF EXISTS `Devices`;

CREATE TABLE `Devices` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `RequestDeviceId` bigint(20) DEFAULT NULL,
  `IsActived` tinyint(1) NOT NULL,
  `ClientId` varchar(200) NOT NULL DEFAULT 'MSoatVe',
  `DeviceKey` varchar(200) NOT NULL,
  `DeviceSecret` varchar(200) NOT NULL,
  `DeviceName` varchar(500) NOT NULL,
  `DeviceDescription` varchar(500) DEFAULT NULL,
  `SerialNumber` varchar(200) DEFAULT NULL,
  `IMEI` varchar(200) DEFAULT NULL,
  `Manufacturer` varchar(200) DEFAULT NULL,
  `Model` varchar(200) DEFAULT NULL,
  `Platform` varchar(200) DEFAULT NULL,
  `PlatformVersion` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=112 DEFAULT CHARSET=utf8;

/*Table structure for table `GroupPermissions` */

DROP TABLE IF EXISTS `GroupPermissions`;

CREATE TABLE `GroupPermissions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `GroupId` int(11) NOT NULL,
  `PermissionId` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=74 DEFAULT CHARSET=utf8;

/*Table structure for table `GroupTerms` */

DROP TABLE IF EXISTS `GroupTerms`;

CREATE TABLE `GroupTerms` (
  `GroupId` int(11) NOT NULL,
  `TermId` int(11) NOT NULL,
  PRIMARY KEY (`GroupId`,`TermId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `GroupUsers` */

DROP TABLE IF EXISTS `GroupUsers`;

CREATE TABLE `GroupUsers` (
  `GroupId` int(11) NOT NULL,
  `UserId` int(11) NOT NULL,
  `UserGuid` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`GroupId`,`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `Groups` */

DROP TABLE IF EXISTS `Groups`;

CREATE TABLE `Groups` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(256) NOT NULL,
  `Description` text,
  `ParentId` int(11) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=263 DEFAULT CHARSET=utf8;

/*Table structure for table `LoginHistory` */

DROP TABLE IF EXISTS `LoginHistory`;

CREATE TABLE `LoginHistory` (
  `Id` bigint(20) NOT NULL AUTO_INCREMENT,
  `Type` int(11) NOT NULL,
  `UserName` varchar(45) DEFAULT NULL,
  `LoginTime` datetime NOT NULL,
  `LoginStatus` smallint(6) NOT NULL,
  `AppId` varchar(200) DEFAULT NULL,
  `RefreshToken` varchar(200) DEFAULT NULL,
  `ClientUri` varchar(500) DEFAULT NULL,
  `ClientIP` varchar(45) DEFAULT NULL,
  `ClientUA` varchar(500) DEFAULT NULL,
  `ClientDevice` varchar(100) DEFAULT NULL,
  `ClientApiKey` varchar(200) DEFAULT NULL,
  `Created` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=MyISAM AUTO_INCREMENT=25812 DEFAULT CHARSET=utf8;

/*Table structure for table `PermissionGrants` */

DROP TABLE IF EXISTS `PermissionGrants`;

CREATE TABLE `PermissionGrants` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PermissionId` varchar(45) NOT NULL,
  `Type` mediumint(4) NOT NULL,
  `IsExactPattern` tinyint(1) NOT NULL,
  `TermPattern` varchar(200) DEFAULT NULL,
  `TermExactPattern` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=107 DEFAULT CHARSET=utf8;

/*Table structure for table `Permissions` */

DROP TABLE IF EXISTS `Permissions`;

CREATE TABLE `Permissions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=utf8;

/*Table structure for table `RefreshTokens` */

DROP TABLE IF EXISTS `RefreshTokens`;

CREATE TABLE `RefreshTokens` (
  `Id` varchar(200) NOT NULL,
  `Subject` varchar(50) NOT NULL,
  `ClientId` varchar(50) DEFAULT NULL,
  `IssuedUtc` datetime NOT NULL,
  `ExpiresUtc` datetime NOT NULL,
  `ProtectedTicket` text NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `RequestDevices` */

DROP TABLE IF EXISTS `RequestDevices`;

CREATE TABLE `RequestDevices` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `IsApproved` tinyint(1) NOT NULL,
  `ClientId` varchar(200) NOT NULL DEFAULT 'MSoatVe',
  `DeviceKey` varchar(200) NOT NULL,
  `SerialNumber` varchar(200) DEFAULT NULL,
  `IMEI` varchar(200) DEFAULT NULL,
  `Manufacturer` varchar(200) DEFAULT NULL,
  `Model` varchar(200) DEFAULT NULL,
  `Platform` varchar(200) DEFAULT NULL,
  `PlatformVersion` varchar(200) DEFAULT NULL,
  `Created` datetime NOT NULL,
  `Updated` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=113 DEFAULT CHARSET=utf8;

/*Table structure for table `Roles` */

DROP TABLE IF EXISTS `Roles`;

CREATE TABLE `Roles` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Guid` varchar(128) DEFAULT NULL,
  `Name` varchar(256) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=107 DEFAULT CHARSET=utf8;

/*Table structure for table `Terms` */

DROP TABLE IF EXISTS `Terms`;

CREATE TABLE `Terms` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `RoleKey` varchar(500) NOT NULL,
  `Name` varchar(500) NOT NULL,
  `Description` text,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=137 DEFAULT CHARSET=utf8;

/*Table structure for table `TrustDevices` */

DROP TABLE IF EXISTS `TrustDevices`;

CREATE TABLE `TrustDevices` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `DeviceSerial` varchar(500) NOT NULL,
  `DeviceGroup` int(11) NOT NULL,
  `CreatedTime` datetime NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=389 DEFAULT CHARSET=utf8;

/*Table structure for table `UserApps` */

DROP TABLE IF EXISTS `UserApps`;

CREATE TABLE `UserApps` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) NOT NULL,
  `IsActive` int(11) NOT NULL DEFAULT '0',
  `ApiType` int(11) NOT NULL DEFAULT '0',
  `ApiName` varchar(200) NOT NULL,
  `ApiKey` varchar(200) DEFAULT NULL,
  `ApiSecret` varchar(500) DEFAULT NULL,
  `AppHosts` mediumtext,
  `AppIps` mediumtext,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=23 DEFAULT CHARSET=utf8;

/*Table structure for table `UserClaims` */

DROP TABLE IF EXISTS `UserClaims`;

CREATE TABLE `UserClaims` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) NOT NULL,
  `UserGuid` varchar(128) DEFAULT NULL,
  `ClaimType` longtext,
  `ClaimValue` longtext,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id` (`Id`),
  KEY `UserId` (`UserGuid`)
) ENGINE=InnoDB AUTO_INCREMENT=1126 DEFAULT CHARSET=utf8;

/*Table structure for table `UserLogins` */

DROP TABLE IF EXISTS `UserLogins`;

CREATE TABLE `UserLogins` (
  `LoginProvider` varchar(128) NOT NULL,
  `ProviderKey` varchar(128) NOT NULL,
  `UserId` int(11) NOT NULL,
  `UserGuid` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`,`UserId`),
  KEY `ApplicationUser_Logins` (`UserGuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `UserPermissions` */

DROP TABLE IF EXISTS `UserPermissions`;

CREATE TABLE `UserPermissions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `UserId` int(11) NOT NULL,
  `PermissionId` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=89 DEFAULT CHARSET=utf8;

/*Table structure for table `UserRoles` */

DROP TABLE IF EXISTS `UserRoles`;

CREATE TABLE `UserRoles` (
  `UserId` int(11) NOT NULL,
  `UserGuid` varchar(128) DEFAULT NULL,
  `RoleId` int(11) NOT NULL,
  `RoleGuid` varchar(128) DEFAULT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IdentityRole_Users` (`RoleGuid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `UserTerms` */

DROP TABLE IF EXISTS `UserTerms`;

CREATE TABLE `UserTerms` (
  `UserId` int(11) NOT NULL,
  `UserGuid` varchar(128) DEFAULT NULL,
  `TermId` int(11) NOT NULL,
  `IsAccess` tinyint(1) NOT NULL,
  PRIMARY KEY (`UserId`,`TermId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

/*Table structure for table `Users` */

DROP TABLE IF EXISTS `Users`;

CREATE TABLE `Users` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Guid` varchar(128) DEFAULT NULL,
  `Email` varchar(256) DEFAULT NULL,
  `EmailConfirmed` tinyint(1) NOT NULL,
  `PasswordHash` longtext,
  `SecurityStamp` longtext,
  `PhoneNumber` longtext,
  `PhoneNumberConfirmed` tinyint(1) NOT NULL,
  `TwoFactorEnabled` tinyint(1) NOT NULL,
  `LockoutEndDateUtc` datetime DEFAULT NULL,
  `LockoutEnabled` tinyint(1) NOT NULL,
  `AccessFailedCount` int(11) NOT NULL,
  `UserName` varchar(256) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=746 DEFAULT CHARSET=utf8;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
