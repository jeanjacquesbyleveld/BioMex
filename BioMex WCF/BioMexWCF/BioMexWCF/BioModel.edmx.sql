
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 09/16/2015 09:37:12
-- Generated from EDMX file: I:\Honours\Project\#Demo Prototype\BioMex WCF\BioMexWCF\BioMexWCF\BioModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BioDatabase];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK__FaceFeatu__FF_ID__24927208]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FaceFeature] DROP CONSTRAINT [FK__FaceFeatu__FF_ID__24927208];
GO
IF OBJECT_ID(N'[dbo].[FK__Features__US_ID__1273C1CD]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Features] DROP CONSTRAINT [FK__Features__US_ID__1273C1CD];
GO
IF OBJECT_ID(N'[dbo].[FK__KeyLatenc__KL_ID__1B0907CE]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[KeyLatencies] DROP CONSTRAINT [FK__KeyLatenc__KL_ID__1B0907CE];
GO
IF OBJECT_ID(N'[dbo].[FK__KeyOrder__KO_ID__182C9B23]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[KeyOrder] DROP CONSTRAINT [FK__KeyOrder__KO_ID__182C9B23];
GO
IF OBJECT_ID(N'[dbo].[FK__KeyPress__KP_ID__1DE57479]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[KeyPress] DROP CONSTRAINT [FK__KeyPress__KP_ID__1DE57479];
GO
IF OBJECT_ID(N'[dbo].[FK__PairedKey__PK_ID__15502E78]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PairedKeys] DROP CONSTRAINT [FK__PairedKey__PK_ID__15502E78];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FaceFeature]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FaceFeature];
GO
IF OBJECT_ID(N'[dbo].[Features]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Features];
GO
IF OBJECT_ID(N'[dbo].[KeyLatencies]', 'U') IS NOT NULL
    DROP TABLE [dbo].[KeyLatencies];
GO
IF OBJECT_ID(N'[dbo].[KeyOrder]', 'U') IS NOT NULL
    DROP TABLE [dbo].[KeyOrder];
GO
IF OBJECT_ID(N'[dbo].[KeyPress]', 'U') IS NOT NULL
    DROP TABLE [dbo].[KeyPress];
GO
IF OBJECT_ID(N'[dbo].[PairedKeys]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PairedKeys];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'FaceFeatures'
CREATE TABLE [dbo].[FaceFeatures] (
    [FF_ID] int  NOT NULL,
    [FF_X_COORDINATE] int  NOT NULL,
    [FF_Y_COORDINATE] int  NOT NULL,
    [FF_DESCRIPTOR] varchar(max)  NOT NULL
);
GO

-- Creating table 'Features'
CREATE TABLE [dbo].[Features] (
    [FE_ID] int  NOT NULL,
    [US_ID] int  NOT NULL,
    [FE_TIME_TAKEN] datetime  NOT NULL,
    [FE_TYPING_SPEED] int  NOT NULL,
    [FE_SHIFT_CLASS] int  NOT NULL,
    [FE_PASSWORD_COUNT] int  NOT NULL
);
GO

-- Creating table 'KeyLatencies'
CREATE TABLE [dbo].[KeyLatencies] (
    [KL_ID] int  NOT NULL,
    [KL_1] int  NOT NULL,
    [KL_2] int  NOT NULL,
    [KL_3] int  NOT NULL,
    [KL_4] int  NOT NULL,
    [KL_5] int  NOT NULL,
    [KL_6] int  NOT NULL,
    [KL_7] int  NOT NULL,
    [KL_8] int  NOT NULL,
    [KL_9] int  NOT NULL,
    [KL_10] int  NULL,
    [KL_11] int  NULL,
    [KL_12] int  NULL,
    [KL_13] int  NULL,
    [KL_14] int  NULL,
    [KL_15] int  NULL,
    [KL_16] int  NULL,
    [KL_17] int  NULL,
    [KL_18] int  NULL,
    [KL_19] int  NULL,
    [KL_20] int  NULL
);
GO

-- Creating table 'KeyOrders'
CREATE TABLE [dbo].[KeyOrders] (
    [KO_ID] int  NOT NULL,
    [KO_1] int  NOT NULL,
    [KO_2] int  NOT NULL,
    [KO_3] int  NOT NULL,
    [KO_4] int  NOT NULL,
    [KO_5] int  NOT NULL,
    [KO_6] int  NOT NULL,
    [KO_7] int  NOT NULL,
    [KO_8] int  NOT NULL,
    [KO_9] int  NOT NULL,
    [KO_10] int  NULL,
    [KO_11] int  NULL,
    [KO_12] int  NULL,
    [KO_13] int  NULL,
    [KO_14] int  NULL,
    [KO_15] int  NULL,
    [KO_16] int  NULL,
    [KO_17] int  NULL,
    [KO_18] int  NULL,
    [KO_19] int  NULL,
    [KO_20] int  NULL
);
GO

-- Creating table 'KeyPresses'
CREATE TABLE [dbo].[KeyPresses] (
    [KP_ID] int  NOT NULL,
    [KP_1] int  NOT NULL,
    [KP_2] int  NOT NULL,
    [KP_3] int  NOT NULL,
    [KP_4] int  NOT NULL,
    [KP_5] int  NOT NULL,
    [KP_6] int  NOT NULL,
    [KP_7] int  NOT NULL,
    [KP_8] int  NOT NULL,
    [KP_9] int  NOT NULL,
    [KP_10] int  NULL,
    [KP_11] int  NULL,
    [KP_12] int  NULL,
    [KP_13] int  NULL,
    [KP_14] int  NULL,
    [KP_15] int  NULL,
    [KP_16] int  NULL,
    [KP_17] int  NULL,
    [KP_18] int  NULL,
    [KP_19] int  NULL,
    [KP_20] int  NULL
);
GO

-- Creating table 'PairedKeys'
CREATE TABLE [dbo].[PairedKeys] (
    [PK_ID] int  NOT NULL,
    [PK_1] int  NOT NULL,
    [PK_2] int  NOT NULL,
    [PK_3] int  NOT NULL,
    [PK_4] int  NULL,
    [PK_5] int  NULL,
    [PK_6] int  NULL,
    [PK_7] int  NULL,
    [PK_8] int  NULL,
    [PK_9] int  NULL,
    [PK_10] int  NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [US_ID] int  NOT NULL,
    [US_NAME] varchar(max)  NULL,
    [US_SURNAME] varchar(max)  NULL,
    [US_USERNAME] varchar(max)  NOT NULL,
    [US_AGE] int  NULL,
    [US_UP_TO_DATE] bit  NOT NULL,
    [US_LAST_LOGGED_IN] datetime  NOT NULL,
    [US_IS_ADMIN] bit  NOT NULL,
    [US_PASSWORD] varchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [FF_ID] in table 'FaceFeatures'
ALTER TABLE [dbo].[FaceFeatures]
ADD CONSTRAINT [PK_FaceFeatures]
    PRIMARY KEY CLUSTERED ([FF_ID] ASC);
GO

-- Creating primary key on [FE_ID] in table 'Features'
ALTER TABLE [dbo].[Features]
ADD CONSTRAINT [PK_Features]
    PRIMARY KEY CLUSTERED ([FE_ID] ASC);
GO

-- Creating primary key on [KL_ID] in table 'KeyLatencies'
ALTER TABLE [dbo].[KeyLatencies]
ADD CONSTRAINT [PK_KeyLatencies]
    PRIMARY KEY CLUSTERED ([KL_ID] ASC);
GO

-- Creating primary key on [KO_ID] in table 'KeyOrders'
ALTER TABLE [dbo].[KeyOrders]
ADD CONSTRAINT [PK_KeyOrders]
    PRIMARY KEY CLUSTERED ([KO_ID] ASC);
GO

-- Creating primary key on [KP_ID] in table 'KeyPresses'
ALTER TABLE [dbo].[KeyPresses]
ADD CONSTRAINT [PK_KeyPresses]
    PRIMARY KEY CLUSTERED ([KP_ID] ASC);
GO

-- Creating primary key on [PK_ID] in table 'PairedKeys'
ALTER TABLE [dbo].[PairedKeys]
ADD CONSTRAINT [PK_PairedKeys]
    PRIMARY KEY CLUSTERED ([PK_ID] ASC);
GO

-- Creating primary key on [US_ID] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([US_ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [FF_ID] in table 'FaceFeatures'
ALTER TABLE [dbo].[FaceFeatures]
ADD CONSTRAINT [FK__FaceFeatu__FF_ID__24927208]
    FOREIGN KEY ([FF_ID])
    REFERENCES [dbo].[Features]
        ([FE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [US_ID] in table 'Features'
ALTER TABLE [dbo].[Features]
ADD CONSTRAINT [FK__Features__US_ID__1273C1CD]
    FOREIGN KEY ([US_ID])
    REFERENCES [dbo].[Users]
        ([US_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK__Features__US_ID__1273C1CD'
CREATE INDEX [IX_FK__Features__US_ID__1273C1CD]
ON [dbo].[Features]
    ([US_ID]);
GO

-- Creating foreign key on [KL_ID] in table 'KeyLatencies'
ALTER TABLE [dbo].[KeyLatencies]
ADD CONSTRAINT [FK__KeyLatenc__KL_ID__1B0907CE]
    FOREIGN KEY ([KL_ID])
    REFERENCES [dbo].[Features]
        ([FE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [KO_ID] in table 'KeyOrders'
ALTER TABLE [dbo].[KeyOrders]
ADD CONSTRAINT [FK__KeyOrder__KO_ID__182C9B23]
    FOREIGN KEY ([KO_ID])
    REFERENCES [dbo].[Features]
        ([FE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [KP_ID] in table 'KeyPresses'
ALTER TABLE [dbo].[KeyPresses]
ADD CONSTRAINT [FK__KeyPress__KP_ID__1DE57479]
    FOREIGN KEY ([KP_ID])
    REFERENCES [dbo].[Features]
        ([FE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [PK_ID] in table 'PairedKeys'
ALTER TABLE [dbo].[PairedKeys]
ADD CONSTRAINT [FK__PairedKey__PK_ID__15502E78]
    FOREIGN KEY ([PK_ID])
    REFERENCES [dbo].[Features]
        ([FE_ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------