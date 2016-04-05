USE [clouddr-control-db]
GO

/****** Object:  StoredProcedure [dbo].[usp_UpdateControlDetailStatus]    Script Date: 4/5/2016 3:48:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[usp_UpdateControlDetailStatus]
	@ControlProcess int = 1, 
	@Id int = 1,
	@Status int = 1 --NP (Not Processed)
AS
BEGIN
if(@ControlProcess = 1)
Begin
 Update ETLControlDetail Set PrimaryAPSStatus = @Status Where id = @Id
end
Else
begin
 Update ETLControlDetail Set SecondaryAPSStatus = @Status Where id = @Id
End
END



GO


