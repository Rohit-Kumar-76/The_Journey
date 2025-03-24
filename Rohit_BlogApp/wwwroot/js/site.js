







document.addEventListener("DOMContentLoaded", function () {
    // Check all posts' content
    const posts = document.querySelectorAll('[id^="post-content-"]');
    posts.forEach(post => {
        // If content overflows, show "View More" button
        if (post.scrollHeight > post.clientHeight) {
            const postId = post.getAttribute('id').split('-')[2];
            const viewMoreBtn = document.getElementById('view-more-' + postId);
            if (viewMoreBtn) {
                viewMoreBtn.style.display = 'inline';
            }
        }
    });
});

function likePost(postId) {
    fetch(`/Post/Like?postId=${postId}`, {
        method: 'POST'
    })
        .then(response => response.json())
        .then(data => {
            document.getElementById(`like-count-${postId}`).innerText = data.likes;
        })
        .catch(error => console.error("Error:", error));
}


function toggleContent(postId) {
    const content = document.getElementById('post-content-' + postId);
    const button = document.getElementById('view-more-' + postId);

    if (content.classList.contains('line-clamp-3')) {
        content.classList.remove('line-clamp-3', 'max-h-20', 'overflow-hidden');
        button.textContent = 'View Less';
    } else {
        content.classList.add('line-clamp-3', 'max-h-20', 'overflow-hidden');
        button.textContent = 'View More';
    }
}

