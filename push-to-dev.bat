@echo off
echo Force pushing to dev-new...

REM Temporarily ignore this batch file from being tracked
git update-index --assume-unchanged push-to-dev-new.bat

REM Add all files except this batch file
git add .
git reset push-to-dev-new.bat

REM Commit changes
git commit -m "v3 beta"

REM Create (or rename to) dev-new branch
git branch -M dev-new

REM Force push to origin/dev-new
git push origin dev-new --force

REM (Optional) Re-enable tracking the batch file if needed
REM git update-index --no-assume-unchanged push-to-dev-new.bat

echo Dev-new branch force-pushed.
pause
