USE [clouddr-control-db]
GO

/****** Object:  StoredProcedure [dbo].[usp_UpdateToArchiveStatus]    Script Date: 4/5/2016 3:48:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_UpdateToArchiveStatus]
	@ControlDetailId int = 1,
	@FileName varchar(500)
AS
BEGIN
Update [dbo].[ETLControlDetail] Set SecondaryAPSStatus = 4, PrimaryAPSStatus = 4, status = 3, FileName = @FileName Where ID = @ControlDetailId
END
GO


