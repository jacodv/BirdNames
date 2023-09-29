FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /BirdNames

# Add mongodb:
# RUN echo 'http://dl-cdn.alpinelinux.org/alpine/v3.6/main' >> /etc/apk/repositories
# RUN echo 'http://dl-cdn.alpinelinux.org/alpine/v3.6/community' >> /etc/apk/repositories
# RUN apk update
# RUN apk add mongodb mongodb-tools
# RUN mkdir -p /data/db/
# RUN chown root:root /data/db/
# RUN apk add openrc --no-cach
# RUN rc-update add mongodb default
# RUN mkdir /run/openrc/
# RUN touch /run/openrc/softlevel
# RUN rc-service mongodb start



COPY ./output .
# ENTRYPOINT ["mongod"]

EXPOSE 80
EXPOSE 443
# EXPOSE 27107
