USE [db_MonitoreoPing];
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmailRecipients]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EmailRecipients](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Email] [nvarchar](255) NOT NULL,
        [Name] [nvarchar](200) NULL,
        [IsActive] [bit] NOT NULL CONSTRAINT [DF_EmailRecipients_IsActive] DEFAULT (1),
        CONSTRAINT [PK_EmailRecipients] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    -- Insert default recipient
    INSERT INTO [dbo].[EmailRecipients] (Email, Name) VALUES ('paul.ramos@mop.gob.sv', 'Paul Ramos');
END
GO

CREATE OR ALTER PROCEDURE [dbo].[pa_ObtenerDestinatariosActivos]
AS
BEGIN
    SELECT Id, Email, Name, IsActive FROM [dbo].[EmailRecipients] WHERE IsActive = 1;
END
GO
