USE [TF2Pug]
GO
/****** Object:  Table [dbo].[Servers]    Script Date: 08/21/2011 10:59:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Servers](
	[UniqueId] [uniqueidentifier] NOT NULL,
	[Address] [nvarchar](255) NOT NULL,
	[Port] [int] NULL,
	[Password] [nvarchar](50) NOT NULL,
	[RconPassword] [nvarchar](50) NOT NULL,
	[FriendlyName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Servers] PRIMARY KEY CLUSTERED 
(
	[UniqueId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PUGs]    Script Date: 08/21/2011 10:59:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PUGs](
	[UniqueId] [uniqueidentifier] NOT NULL,
	[MapId] [uniqueidentifier] NOT NULL,
	[BluScore] [tinyint] NOT NULL,
	[RedScore] [tinyint] NOT NULL,
	[Started] [datetime] NOT NULL,
 CONSTRAINT [PK_PUGs] PRIMARY KEY CLUSTERED 
(
	[UniqueId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PugPlayers]    Script Date: 08/21/2011 10:59:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PugPlayers](
	[PugId] [uniqueidentifier] NOT NULL,
	[PlayerId] [nvarchar](255) NOT NULL,
	[PlayerClass] [tinyint] NOT NULL,
 CONSTRAINT [PK_PugPlayers] PRIMARY KEY CLUSTERED 
(
	[PugId] ASC,
	[PlayerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Players]    Script Date: 08/21/2011 10:59:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Players](
	[Id] [nvarchar](255) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Players] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Maps]    Script Date: 08/21/2011 10:59:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Maps](
	[UniqueId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[FriendlyName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Maps] PRIMARY KEY CLUSTERED 
(
	[UniqueId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[sp_ReportScore]    Script Date: 08/21/2011 10:59:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_ReportScore]
	@PugId uniqueidentifier,
	@BluScore tinyint,
	@RedScore tinyint
AS
BEGIN
	SET NOCOUNT ON;

	IF (SELECT COUNT(1) FROM PUGs WHERE UniqueId = @PugId AND BluScore = 0 AND RedScore = 0) > 0
	BEGIN
		UPDATE PUGs SET BluScore = @BluScore, RedScore = @RedScore WHERE UniqueId = @PugId;
	END
END
GO
/****** Object:  StoredProcedure [dbo].[sp_AddPlayerToPug]    Script Date: 08/21/2011 10:59:46 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_AddPlayerToPug]
	@PugId uniqueidentifier,
	@PlayerId nvarchar(255),
	@PlayerName nvarchar(255),
	@PlayerClass tinyint
AS
BEGIN
	SET NOCOUNT ON;

    IF (SELECT COUNT(1) FROM Players WHERE [Id] = @PlayerId) = 0
    BEGIN
		INSERT INTO Players ([Id], Name) VALUES (@PlayerId, @PlayerName);
    END
    ELSE IF (SELECT COUNT(1) FROM Players WHERE [Id] = @PlayerId AND Name = @PlayerName) = 0
    BEGIN
		UPDATE Players SET Name = @PlayerName WHERE [Id] = @PlayerId;
	END
	
	INSERT INTO PugPlayers (PugId, PlayerId, PlayerClass) VALUES (@PugId, @PlayerId, @PlayerClass);
END
GO
