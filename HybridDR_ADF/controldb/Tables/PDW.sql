USE [clouddr-control-db]
GO

/****** Object:  Table [dbo].[PDW]    Script Date: 4/5/2016 3:58:57 PM ******/
/**PDW – Identifies the source the Destination PDW connection information **/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[PDW](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PDWName] [nchar](10) NOT NULL,
	[PrimaryPDW] [nchar](10) NULL,
	[PDWIPAddress] [varchar](50) NOT NULL,
 CONSTRAINT [PK__PDW__3214EC070E6005B6] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO

SET ANSI_PADDING OFF
GO


