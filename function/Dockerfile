# FROM microsoft/dotnet:2.2-sdk AS installer-env

# RUN git 


# COPY . /src/dotnet-function-app
# RUN cd /src/dotnet-function-app && \
#     mkdir -p /home/site/wwwroot && \
#     dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
FROM mcr.microsoft.com/azure-functions/dotnet:2.0-appservice 
#FROM mcr.microsoft.com/azure-functions/dotnet:2.0
# RUN apt-get update && apt-get -y install openssh-server

RUN apt-get -y install git && git config --global user.email "thenickfish@protonmail.com" &&  git config --global user.name "Nick Fish"


# RUN chmod u+w ~
# COPY --chown=root id_rsa /root/.ssh/id_rsa
# RUN chmod 700 /root/.ssh && chmod 700 /root/.ssh/id_rsa
# RUN eval $(ssh-agent -s) && ssh-add /root/.ssh/id_rsa && ssh-add -l

RUN mkdir ~/.ssh && ssh-keygen -t rsa -f ~/.ssh/id_rsa -N ''
RUN cat ~/.ssh/id_rsa.pub
RUN eval $(ssh-agent -s) && ssh-add ~/.ssh/id_rsa && ssh-add -l && git clone git@github.com:thenickfish/recipes.git
RUN ls -al ~/.ssh
RUN ssh -vT git@github.com

# RUN ssh -o StrictHostKeyChecking=no -vT github.com
# RUN ssh -T git@github.com

# RUN ps aux | grep sshd
# RUN ls ~
# RUN ssh -T git@github.com

RUN git clone git@github.com:thenickfish/recipes.git
RUN touch recipes/test && cd recipes && git add -A && git commit -m "test" && git push

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]
