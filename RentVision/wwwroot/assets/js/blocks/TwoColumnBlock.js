/*global
    piranha
*/

Vue.component("two-column-block", {
    props: ["uid", "toolbar", "model"],
    data: function () {
        return {
            column1: this.model.column1.value,
            column2: this.model.column2.value
        };
    },
    methods: {
        onTitleBlur: function (e) {
            this.model.title.value = e.target.innerText;
        },

        onSubtitleBlur: function (e) {
            this.model.subtitle.value = e.target.innerText;
        },

        onBlurCol1: function (e) {
            this.model.column1.value = e.target.innerHTML;
        },

        onBlurCol2: function (e) {
            this.model.column2.value = e.target.innerHTML;
        }
    },
    computed: {
        isTitleEmpty: function () {
            return piranha.utils.isEmptyText(this.model.title.value);
        },

        isSubtitleEmpty: function () {
            return piranha.utils.isEmptyText(this.model.subtitle.value);
        },

        isEmptyCol1: function () {
            return piranha.utils.isEmptyHtml(this.model.column1.value);
        },

        isEmptyCol2: function () {
            return piranha.utils.isEmptyHtml(this.model.column2.value);
        }
    },
    mounted: function () {
        piranha.editor.addInline(this.uid + 1, this.toolbar);
        piranha.editor.addInline(this.uid + 2, this.toolbar);
    },
    beforeDestroy: function () {
        piranha.editor.remove(this.uid + 1);
        piranha.editor.remove(this.uid + 2);
    },
    template:
        `<div class='block block-body'>
            <h3 contenteditable='true' v-html='model.title.value' v-on:blur='onTitleBlur' :class='{ empty: isTitleEmpty}'>Lorem ipsum dolor sit amet</h3>
        	<div class='divider'></div>
        	<div class='row'>
        		<div class='col-md-6'>
        			<div class='block-left'>
        				<h4 contenteditable='true' spellcheck='false' v-html='model.subtitle.value' v-on:blur='onSubtitleBlur' :class='{ empty: isSubtitleEmpty}'><i class='fas fa-check'></i>All-in-one</h4>
                		
                        <div :class='{ empty: isEmptyCol1 }'>
                            <div :id='uid + 1' contenteditable='true' spellcheck='false' v-html='column1' v-on:blur='onBlurCol1'></div>
                        </div>
                	</div>
                </div>
                <div class='col-md-6'>
                    <div :class='{ empty: isEmptyCol2 }'>
                        <div :id='uid + 2' contenteditable='true' spellcheck='false' v-html='column2' v-on:blur='onBlurCol2'></div>
                    </div>
                </div>
            </div>
        </div>`
});