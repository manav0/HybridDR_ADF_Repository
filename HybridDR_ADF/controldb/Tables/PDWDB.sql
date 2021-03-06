﻿USE [clouddr-control-db]
GO

/****** Object:  Table [dbo].[PDWDB]    Script Date: 4/5/2016 3:59:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PDWDB](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PDWID] [int] NOT NULL,
	[DBName] [varchar](128) NOT NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_PDWDB] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE [dbo].[PDWDB]  WITH CHECK ADD  CONSTRAINT [FK_PDWDB_PDW] FOREIGN KEY([PDWID])
REFERENCES [dbo].[PDW] ([Id])
GO

ALTER TABLE [dbo].[PDWDB] CHECK CONSTRAINT [FK_PDWDB_PDW]
GO


