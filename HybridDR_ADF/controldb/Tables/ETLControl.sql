USE [clouddr-control-db]
GO

/****** Object:  Table [dbo].[ETLControl]    Script Date: 4/5/2016 3:51:55 PM ******/
/** ETLControl – This table stores the configuration details for performing the dual load process for each ETL process. **/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[ETLControl](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ControlProcess] [varchar](128) NOT NULL,
	[LastRunDate] [datetime] NOT NULL,
	[FileNameLike] [varchar](128) NULL,
	[FilePath] [varchar](500) NULL,
	[ToBeProcessedPath] [varchar](500) NULL,
	[ArchivePath] [varchar](500) NULL,
 CONSTRAINT [PK_ETLControl] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

SET ANSI_PADDING OFF
GO


