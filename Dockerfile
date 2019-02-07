FROM ubuntu:18.04

RUN useradd -s /bin/bash mecha
USER mecha

COPY ./Client/mechahamster_client_Data /opt/mechahamster/mechahamster_client_Data
COPY ./Client/mechahamster_client.x86_64 /opt/mechahamster/mechahamster_client.x86_64
COPY ./Client/remote_config_data /opt/mechahamster/remote_config_data

EXPOSE 7777/udp
CMD ["/opt/mechahamster/mechahamster_client.x86_64", "-s", "-nographics"]
