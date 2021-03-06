CREATE VIEW [View_BookingSystem_Joined]
AS
SELECT     SiteName, ClassName, ClassDisplayName, NodeID, NodeAliasPath, NodeName, NodeLinkedNodeID, NodeAlias, NodeGroupID, DocumentID, 
                      DocumentName, DocumentNamePath, DocumentCulture, DocumentForeignKeyValue,
                          (SELECT     COUNT(*) AS Count
                            FROM          dbo.Events_Attendee
                            WHERE      (AttendeeEventNodeID = dbo.View_CMS_Tree_Joined.NodeID)) AS AttendeesCount
FROM         dbo.View_CMS_Tree_Joined
WHERE     (ClassName = 'CMS.BookingEvent')
GO
