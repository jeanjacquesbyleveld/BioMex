
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 08/20/2015 10:52:20
-- Generated from EDMX file: I:\Honours\Project\#Demo Prototype\BioMex WCF\BioMexWCF\BioMexWCF\BioMexEntities.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BioMexDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[KeyStrokeFeatures]', 'U') IS NOT NULL
    DROP TABLE [dbo].[KeyStrokeFeatures];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'KeyStrokeFeatures'
CREATE TABLE [dbo].[KeyStrokeFeatures] (
    [KF_ID] int IDENTITY(1,1) NOT NULL,
    [KF_TimeTaken] datetime  NULL,
    [KF_TypingSpeed] int  NULL,
    [KF_ShiftClassification] int  NULL,
    [UserUS_ID] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [US_ID] int IDENTITY(1,1) NOT NULL,
    [US_Name] varchar(max)  NULL,
    [US_Surname] varchar(max)  NULL,
    [US_Age] int  NULL,
    [US_UpToDate] bit  NULL,
    [US_DateLastLoggedIn] datetime  NULL,
    [US_Username] varchar(max)  NOT NULL,
    [US_Password] varchar(max)  NOT NULL,
    [US_PasswordCount] int  NOT NULL,
    [US_Administrator] bit  NOT NULL,
    [KF_ID] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [KF_ID] in table 'KeyStrokeFeatures'
ALTER TABLE [dbo].[KeyStrokeFeatures]
ADD CONSTRAINT [PK_KeyStrokeFeatures]
    PRIMARY KEY CLUSTERED ([KF_ID] ASC);
GO

-- Creating primary key on [US_ID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([US_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [UserUS_ID] in table 'KeyStrokeFeatures'
ALTER TABLE [dbo].[KeyStrokeFeatures]
ADD CONSTRAINT [FK_UserKeyStrokeFeature]
    FOREIGN KEY ([UserUS_ID])
    REFERENCES [dbo].[Users]
        ([US_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserKeyStrokeFeature'
CREATE INDEX [IX_FK_UserKeyStrokeFeature]
ON [dbo].[KeyStrokeFeatures]
    ([UserUS_ID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------