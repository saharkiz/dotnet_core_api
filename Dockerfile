#---BUILD PHASE-----
#base Image
FROM mcr.microsoft.com/dotnet/sdk:3.1 As build
#setting working directory
WORKDIR /application
#copy files to working directory
COPY *.csproj .
#restore all dependency
RUN dotnet restore
#copy remaining files to working directory
COPY . .
#build and publish
RUN dotnet publish -c Release -o publish

#----RUN PHASE----
#run application on base image
FROM mcr.microsoft.com/dotnet/sdk:3.1 
#Create working dir
WORKDIR /application
#expose port to access application
EXPOSE 80
#copy binaries
COPY --from=build /application/publish .
#entry point run dotnet and then run DLL
ENTRYPOINT [ "dotnet", "myvapi.dll"]

#docker build -t saharkiz/myvapi .
#docker run -p 2001:80 saharkiz/myvapi

#docker login
#docker images
#docker push saharkiz/myvapi

#sudo dotnet publish --configuration Release
#sudo dotnet watch run