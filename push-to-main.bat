@echo off
echo Force pushing to dev...

REM Temporarily ignore this batch file
git update-index --assume-unchanged push-to-dev.bat

git add .
git reset push-to-dev.bat
git commit -m "v3 beta - new categories and more"
git branch -M dev
git push origin dev --force

REM Optionally reset the assume-unchanged flag if needed
REM git update-index --no-assume-unchanged push-to-dev.bat

echo Dev branch force-pushed.
pause
