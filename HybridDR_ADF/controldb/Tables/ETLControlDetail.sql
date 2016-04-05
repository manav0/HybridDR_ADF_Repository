USE [clouddr-control-db]
GO

/****** Object:  Table [dbo].[ETLControlDetail]    Script Date: 4/5/2016 3:52:31 PM ******/
/** ETLControlDetail – Tracks each step of the dual load process for each file.**/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ETLControlDetail](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ETLControlID] [int] NOT NULL,
	[Status] [smallint] NOT NULL,
	[FileName] [nvarchar](500) NOT NULL,
	[PrimaryAPSStatus] [smallint] NULL,
	[SecondaryAPSStatus] [smallint] NULL,
 CONSTRAINT [PK_ETLControlDetail] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

ALTER TABLE [dbo].[ETLControlDetail]  WITH CHECK ADD  CONSTRAINT [FK_ETLControlDetail_ETLControl] FOREIGN KEY([ETLControlID])
REFERENCES [dbo].[ETLControl] ([Id])
GO

ALTER TABLE [dbo].[ETLControlDetail] CHECK CONSTRAINT [FK_ETLControlDetail_ETLControl]
GO

ALTER TABLE [dbo].[ETLControlDetail]  WITH CHECK ADD  CONSTRAINT [FK_ETLControlDetail_ETLStatusCode] FOREIGN KEY([Status])
REFERENCES [dbo].[ETLStatusCode] ([Id])
GO

ALTER TABLE [dbo].[ETLControlDetail] CHECK CONSTRAINT [FK_ETLControlDetail_ETLStatusCode]
GO

ALTER TABLE [dbo].[ETLControlDetail]  WITH CHECK ADD  CONSTRAINT [FK_ETLControlDetail_ETLStatusCode1] FOREIGN KEY([PrimaryAPSStatus])
REFERENCES [dbo].[ETLStatusCode] ([Id])
GO

ALTER TABLE [dbo].[ETLControlDetail] CHECK CONSTRAINT [FK_ETLControlDetail_ETLStatusCode1]
GO

ALTER TABLE [dbo].[ETLControlDetail]  WITH CHECK ADD  CONSTRAINT [FK_ETLControlDetail_ETLStatusCode2] FOREIGN KEY([SecondaryAPSStatus])
REFERENCES [dbo].[ETLStatusCode] ([Id])
GO

ALTER TABLE [dbo].[ETLControlDetail] CHECK CONSTRAINT [FK_ETLControlDetail_ETLStatusCode2]
GO


