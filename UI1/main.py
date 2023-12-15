import sys

import qbittorrentapi

# instantiate a Client using the appropriate WebUI configuration
qbt_client = qbittorrentapi.Client(
    host='localhost',
    port=8080,
    username='admin',
    password='4568527139aA'
)

# the Client will automatically acquire/maintain a logged in state in line with any request.
# therefore, this is not necessary; however, you many want to test the provided login credentials.
try:
    qbt_client.auth_log_in()
except qbittorrentapi.LoginFailed as e:
    print(e)


def add_torrent(url):
    return qbt_client.torrents.add(urls=url)


if __name__ == '__main__':
    print(add_torrent(sys.argv[1]))
