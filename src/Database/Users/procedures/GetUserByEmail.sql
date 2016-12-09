CREATE PROCEDURE GetUserByEmail
	@Email varchar(255)
AS
	SELECT
		Id,
		FirstName,
		LastName,
		Email,
		Password,
		Salt,
		PasswordProfileId,
		IsActive,
		FailedLoginAttempts,
		IsLocked
	FROM dbo.[User]
	WHERE Email = @Email
GO